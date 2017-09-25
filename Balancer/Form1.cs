using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Laserforce
{
    public partial class Form1 : Form
    {
        #region Fields

        private List<string> _Players;
        private Dictionary<string, string> _RedTeam;
        private Dictionary<string, string> _GreenTeam;

        private Admin _Admin;
        private Session _Session;

        private List<string> _space5Positions;

        private List<Player> _redTeam = new List<Player>();
        private List<Player> _greenTeam = new List<Player>();

        private Dictionary<Player, string> _teamToMatch;

        private PositionMode _currentPositionMode = PositionMode.RedTeam;
        private BalanceMode _currentBalanceMode = BalanceMode.SameClasses;

        #endregion Fields

        #region Constructors

        public Form1()
        {
            InitializeComponent();

            _Players = new List<string>();
            _RedTeam = new Dictionary<string, string>();
            _GreenTeam = new Dictionary<string, string>();

            Location = new Point(0, 0);

            _Admin = new Admin();
            _Admin.Initialise();

            LoadPlayersIntoTable();

            _Session = new Session();

            cmbPositionMode.SelectedIndex = 0;
            txtCommanderWeight.Text = "8";
            txtHeavyWeight.Text = "10";
            txtAmmoWeight.Text = "4";
            txtMedicWeight.Text = "3";
            txtScoutWeight.Text = "1";
            txt3HitWeight.Text = "8";
            
            UpdateStats();

            foreach (var player in _Admin.Players)
            {
                cmbUpdatePlayer.Items.Add(new UpdatePlayerItem()
                {
                    Text = player.Tag,
                    Value = player.Number
                });
            }
        }

        private class UpdatePlayerItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        #endregion Constructors

        #region Properties

        public List<string> Space5Positions
        {
            get
            {
                if (_space5Positions == null)
                {
                    _space5Positions = new List<string>();
                    _space5Positions.Add("Comm.");
                    _space5Positions.Add("Heavy");
                    _space5Positions.Add("Scout");
                    _space5Positions.Add("Ammo");
                    _space5Positions.Add("Medic");
                }

                return _space5Positions;
            }
        }

        public PositionMode CurrentPositionMode
        {
            get { return _currentPositionMode; }
            set
            {
                _currentPositionMode = value;

                if (value == PositionMode.RedTeam)
                {
                    lblSetTeam.Text = "Red Team";
                    lblSetTeam.ForeColor = Color.Red;
                    lblMatchingTeam.Text = "Green Team";
                    lblMatchingTeam.ForeColor = Color.FromArgb(64, 255, 0);
                    grdSetTeamPositions.Columns[0].ReadOnly = false;
                }
                else if (value == PositionMode.GreenTeam)
                {
                    lblSetTeam.Text = "Green Team";
                    lblSetTeam.ForeColor = Color.FromArgb(64, 255, 0);
                    lblMatchingTeam.Text = "Red Team";
                    lblMatchingTeam.ForeColor = Color.Red;
                    grdSetTeamPositions.Columns[0].ReadOnly = false;
                }
                else
                {
                    grdSetTeamPositions.Columns[0].ReadOnly = true;
                }
            }
        }

        public BalanceMode CurrentBalanceMode
        {
            get { return _currentBalanceMode; }
            set
            {
                _currentBalanceMode = value;

                switch (value)
                {
                    case BalanceMode.SameClasses:
                        lbl3Hits.Visible = false;
                        txt3HitWeight.Visible = false;
                        lblCommander.Visible = true;
                        txtCommanderWeight.Visible = true;
                        lblHeavy.Visible = true;
                        txtHeavyWeight.Visible = true;
                        break;

                    case BalanceMode.OppositeClasses:
                        lbl3Hits.Visible = true;
                        txt3HitWeight.Visible = true;
                        lblCommander.Visible = false;
                        txtCommanderWeight.Visible = false;
                        lblHeavy.Visible = false;
                        txtHeavyWeight.Visible = false;
                        break;

                    case BalanceMode.Both:
                        lbl3Hits.Visible = true;
                        txt3HitWeight.Visible = true;
                        lblCommander.Visible = true;
                        txtCommanderWeight.Visible = true;
                        lblHeavy.Visible = true;
                        txtHeavyWeight.Visible = true;
                        break;
                }
            }
        }

        #endregion Properties

        #region Methods

        public static Bitmap GetImageFor(string role)
        {
            switch (role)
            {
                case "Ammo":
                    return Properties.Resources.Ammo;

                case "Medic":
                    return Properties.Resources.Medic;

                case "Scout":
                    return Properties.Resources.Scout;

                case "Heavy":
                    return Properties.Resources.Heavy;

                case "Comm.":
                    return Properties.Resources.Commander;

                default:
                    return null;
            }
        }

        #endregion Methods

        #region Private Methods

        private void UpdateStats()
        {
            var request = (HttpWebRequest)WebRequest.Create(@"http://lfstats.com/scorecards/getOverallAverages.json?gametype=social&leagueID=0&centerID=16");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var t = reader.ReadToEnd();
                JObject x = (JObject)JsonConvert.DeserializeObject(t);

                foreach (var item in x.First.First)
                {
                    var name = item.Value<string>("name").ToString();
                    var index1 = name.IndexOf(">");
                    var index2 = name.IndexOf("<", index1);
                    name = name.Substring(index1 + 1, index2 - index1 - 1);

                    var player = _Admin.GetPlayer(name);

                    if (player != null)
                    {
                        player.OverallRank = item.Value<decimal>("avg_avg_mvp");
                        player.CommanderRank = item.Value<decimal>("commander_avg_mvp");
                        player.HeavyRank = item.Value<decimal>("heavy_avg_mvp");
                        player.ScoutRank = item.Value<decimal>("scout_avg_mvp");
                        player.AmmoRank = item.Value<decimal>("ammo_avg_mvp");
                        player.MedicRank = item.Value<decimal>("medic_avg_mvp");

                        JsonWriter.WritePlayerFile(player);
                    }
                }
            }
        }

        private void LoadPlayersIntoTable()
        {
            grdAllPlayers.Rows.Clear();

            foreach (var player in _Admin.Players)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Number });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Tag });
                row.Cells.Add(new DataGridViewCheckBoxCell() { Value = false });
                grdAllPlayers.Rows.Add(row);
            }
        }

        private void LoadTeams()
        {
            grdRedTeam.Rows.Clear();
            grdGreenTeam.Rows.Clear();

            foreach (var player in _redTeam)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.OverallRank });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Tag });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Number });
                row.Cells.Add(new DataGridViewButtonCell() { Value = "v" });

                grdRedTeam.Rows.Add(row);
            }

            foreach (var player in _greenTeam)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.OverallRank });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Tag });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Number });
                row.Cells.Add(new DataGridViewButtonCell() { Value = "^" });

                grdGreenTeam.Rows.Add(row);
            }

            grdSetTeamPositions.Rows.Clear();
            grdMatchingTeamPositions.Rows.Clear();

            foreach (var player in _redTeam)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Tag });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Number });

                grdSetTeamPositions.Rows.Add(row);
            }

            CalculateTeamTotals();
        }

        private void CalculateTeamTotals()
        {
            lblRedTeam.Text = $"Red Team ({(_redTeam.Sum(x => x.OverallRank))})";
            lblGreenTeam.Text = $"Green Team ({(_greenTeam.Sum(x => x.OverallRank))})";
        }

        private string ConvertShortHandToPosition(string value)
        {
            var position = "Scout";

            switch (value.ToLower())
            {
                case "c":
                    position = "Comm.";
                    break;

                case "h":
                    position = "Heavy";
                    break;

                case "a":
                    position = "Ammo";
                    break;

                case "m":
                    position = "Medic";
                    break;

                case "s":
                    position = "Scout";
                    break;
            }

            return position;
        }

        private Dictionary<Player, string> BalancePositions(Dictionary<Player, string> setTeam, List<Player> teamToBalance)
        {
            decimal bestScore = decimal.MaxValue;
            var bestTeam = new Dictionary<Player, string>();

            foreach (var commander in teamToBalance)
            {
                foreach (var heavy in teamToBalance.Where(x => x != commander))
                {
                    foreach (var ammo in teamToBalance.Where(x => x != commander && x != heavy))
                    {
                        foreach (var medic in teamToBalance.Where(x => x != commander && x != heavy && x != ammo))
                        {
                            decimal thisScore = 0;
                            switch (CurrentBalanceMode)
                            {
                                case BalanceMode.SameClasses:
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Comm.").Key, commander, "Comm.");
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Heavy").Key, heavy, "Heavy");
                                    break;

                                case BalanceMode.OppositeClasses:
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Comm.").Key, heavy, "C-H");
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Heavy").Key, commander, "H-C");
                                    break;

                                case BalanceMode.Both:
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Comm.").Key, commander, "Comm.");
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Heavy").Key, heavy, "Heavy");
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Comm.").Key, heavy, "C-H");
                                    thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Heavy").Key, commander, "H-C");
                                    break;
                            }
                            
                            thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Ammo").Key, ammo, "Ammo");
                            thisScore += CalculatePlayerDifference(setTeam.First(x => x.Value == "Medic").Key, medic, "Medic");

                            var setScouts = setTeam.Where(x => x.Value == "Scout").Select(y => y.Key).ToList();
                            var compareScouts = teamToBalance.Where(x => x != commander && x != heavy && x != ammo && x != medic).ToList();
                            thisScore += CompareScouts(setScouts, compareScouts);

                            if (Math.Abs(thisScore) < Math.Abs(bestScore))
                            {
                                bestTeam.Clear();
                                bestScore = thisScore;

                                bestTeam.Add(commander, "Comm.");
                                bestTeam.Add(heavy, "Heavy");
                                bestTeam.Add(ammo, "Ammo");
                                bestTeam.Add(medic, "Medic");

                                foreach (var scout in compareScouts)
                                {
                                    bestTeam.Add(scout, "Scout");
                                }
                            }
                        }
                    }
                }
            }

            if (bestScore > 0M)
            {
                lblFavoured.ForeColor = Color.FromArgb(64, 255, 0);
                lblFavoured.Text = $"Green favoured ({bestScore})";
            }
            else if (bestScore < 0M)
            {
                lblFavoured.ForeColor = Color.Red;
                lblFavoured.Text = $"Red favoured ({Math.Abs(bestScore)})";
            }
            else
            {
                lblFavoured.ForeColor = Color.White;
                lblFavoured.Text = "Neither team favoured";
            }

            return bestTeam;
        }

        private decimal CompareScouts(List<Player> setScouts, List<Player> compareScouts)
        {
            var smallerTeam = Math.Min(setScouts.Count, compareScouts.Count);
            var score = 0M;

            for (int i = 0; i < smallerTeam; ++i)
            {
                score += CalculatePlayerDifference(setScouts[i], compareScouts[i], "Scout");
            }

            if (setScouts.Count > compareScouts.Count)
            {
                for (int i = smallerTeam; i < setScouts.Count; ++i)
                {
                    score -= setScouts[i].ScoutRank;
                }
            }
            else if (compareScouts.Count > setScouts.Count)
            {
                for (int i = smallerTeam; i < compareScouts.Count; ++i)
                {
                    score += compareScouts[i].ScoutRank;
                }
            }

            return score;
        }

        private decimal CalculatePlayerDifference(Player setPlayer, Player comparePlayer, string position)
        {
            decimal setPlayerScore = 0;
            decimal comparePlayerScore = 0;
            decimal positionMultiplier = 1;

            switch (position)
            {
                case "Comm.":
                    setPlayerScore = setPlayer.CommanderRank;
                    comparePlayerScore = comparePlayer.CommanderRank;
                    positionMultiplier = Convert.ToDecimal(txtCommanderWeight.Text);
                    break;

                case "C-H":
                    setPlayerScore = setPlayer.CommanderRank;
                    comparePlayerScore = comparePlayer.HeavyRank;
                    positionMultiplier = Convert.ToDecimal(txt3HitWeight.Text);
                    break;

                case "Heavy":
                    setPlayerScore = setPlayer.HeavyRank;
                    comparePlayerScore = comparePlayer.HeavyRank;
                    positionMultiplier = Convert.ToDecimal(txtHeavyWeight.Text);
                    break;

                case "H-C":
                    setPlayerScore = setPlayer.HeavyRank;
                    comparePlayerScore = comparePlayer.CommanderRank;
                    positionMultiplier = Convert.ToDecimal(txt3HitWeight.Text);
                    break;

                case "Ammo":
                    setPlayerScore = setPlayer.AmmoRank;
                    comparePlayerScore = comparePlayer.AmmoRank;
                    positionMultiplier = Convert.ToDecimal(txtAmmoWeight.Text);
                    break;

                case "Medic":
                    setPlayerScore = setPlayer.MedicRank;
                    comparePlayerScore = comparePlayer.MedicRank;
                    positionMultiplier = Convert.ToDecimal(txtMedicWeight.Text);
                    break;

                case "Scout":
                    setPlayerScore = setPlayer.ScoutRank;
                    comparePlayerScore = comparePlayer.ScoutRank;
                    positionMultiplier = Convert.ToDecimal(txtScoutWeight.Text);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return (comparePlayerScore - setPlayerScore) * positionMultiplier;
        }

        private decimal GetMvpForPosition(Player player, string position)
        {
            var mvp = 0M;

            switch (position)
            {
                case "Comm.":
                    mvp = player.CommanderRank;
                    break;

                case "Heavy":
                    mvp = player.HeavyRank;
                    break;

                case "Scout":
                    mvp = player.ScoutRank;
                    break;

                case "Ammo":
                    mvp = player.AmmoRank;
                    break;

                case "Medic":
                    mvp = player.MedicRank;
                    break;
            }

            return mvp;
        }

        #endregion Private Methods

        #region Event Handlers

        private void grdPlaying_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                var playerId = Convert.ToInt32(grdPlaying[0, e.RowIndex].Value);

                var player = _Admin.GetPlayer(playerId);

                _Session.Playing.Remove(player);

                grdPlaying.Rows.RemoveAt(e.RowIndex);

                for (int i = 0; i < grdAllPlayers.Rows.Count; ++i)
                {
                    if (Convert.ToInt32(grdAllPlayers[0, i].Value) == playerId)
                    {
                        grdAllPlayers[2, i].Value = false;
                        break;
                    }
                }
            }
        }

        private void grdAllPlayers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                var playerId = Convert.ToInt32(grdAllPlayers[0, e.RowIndex].Value);
                var isSelected = Convert.ToBoolean(grdAllPlayers[2, e.RowIndex].Value);

                if (isSelected)
                {
                    var player = _Admin.GetPlayer(playerId);

                    _Session.Playing.Remove(player);

                    for (int i = 0; i < grdPlaying.Rows.Count; ++i)
                    {
                        if (Convert.ToInt32(grdPlaying[0, i].Value) == playerId)
                        {
                            grdPlaying.Rows.RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                {
                    if (_Session.Playing.Count == 22)
                    {
                        MessageBox.Show("Max player count has been reached!");
                        return;
                    }
                    else
                    {
                        var player = _Admin.GetPlayer(playerId);
                        var gameTag = player.Tag;
                        _Session.Playing.Add(player);
                        var row = new DataGridViewRow();
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Number });
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Tag });
                        row.Cells.Add(new DataGridViewButtonCell() { Value = "X" });
                        grdPlaying.Rows.Add(row);
                    }
                }

                grdAllPlayers[2, e.RowIndex].Value = !isSelected;
            }
        }

        private void btnBalance_Click(object sender, EventArgs e)
        {
            _redTeam.Clear();
            _greenTeam.Clear();
            var game = new Game(_Session);
            game.RandomiseTeams(out _redTeam, out _greenTeam);

            LoadTeams();
        }

        private void grdRedTeam_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                var playerId = Convert.ToInt32(grdRedTeam[2, e.RowIndex].Value);
                var player = _Admin.GetPlayer(playerId);

                _redTeam.Remove(player);
                _greenTeam.Add(player);

                LoadTeams();
            }
        }

        private void grdGreenTeam_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                var playerId = Convert.ToInt32(grdGreenTeam[2, e.RowIndex].Value);
                var player = _Admin.GetPlayer(playerId);

                _greenTeam.Remove(player);
                _redTeam.Add(player);

                LoadTeams();
            }
        }

        private void btnSetPositions_Click(object sender, EventArgs e)
        {
            if (_greenTeam.Count + _redTeam.Count < 10)
            {
                MessageBox.Show("You need at least 10 players for Space Marines 5", "Error");
                return;
            }

            if (_greenTeam.Count < 5 || _redTeam.Count < 5)
            {
                MessageBox.Show("You need at least 5 players on each team", "Error");
                return;
            }

            grdMatchingTeamPositions.Rows.Clear();

            _teamToMatch = new Dictionary<Player, string>();

            if (CurrentPositionMode == PositionMode.Randomise)
            {
                var positionOrder = new List<int>();

                var random = new Random(DateTime.Now.Millisecond);

                while (positionOrder.Count < grdSetTeamPositions.Rows.Count)
                {
                    var position = random.Next(0, grdSetTeamPositions.Rows.Count);

                    while (positionOrder.Contains(position))
                    {
                        position = random.Next(0, grdSetTeamPositions.Rows.Count);
                    }

                    positionOrder.Add(position);
                }

                grdSetTeamPositions.Rows[positionOrder[0]].Cells[0].Value = "C";
                grdSetTeamPositions.Rows[positionOrder[1]].Cells[0].Value = "H";
                grdSetTeamPositions.Rows[positionOrder[2]].Cells[0].Value = "A";
                grdSetTeamPositions.Rows[positionOrder[3]].Cells[0].Value = "M";

                for (int i = 4; i < positionOrder.Count; ++i)
                {
                    grdSetTeamPositions.Rows[positionOrder[i]].Cells[0].Value = "S";
                }
            }

            foreach (var row in grdSetTeamPositions.Rows)
            {
                var dataRow = row as DataGridViewRow;
                try
                {
                    var position = ConvertShortHandToPosition(dataRow.Cells[0].Value.ToString());
                    var playerId = dataRow.Cells[3].Value;

                    _teamToMatch.Add(_Admin.GetPlayer(Convert.ToInt32(playerId)), position);
                }
                catch (Exception)
                {
                    MessageBox.Show("One of the positions has been entered incorrectly", "Error");
                    return;
                }
            }

            var balancedTeam = BalancePositions(_teamToMatch, _greenTeam);

            foreach (var player in balancedTeam)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewImageCell() { Value = GetImageFor(player.Value), ImageLayout = DataGridViewImageCellLayout.Zoom });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = GetMvpForPosition(player.Key, player.Value) });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Key.Tag });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Key.Number });

                grdMatchingTeamPositions.Rows.Add(row);
            }
        }

        private void btnSwapTeams_Click(object sender, EventArgs e)
        {
            var newGreenTeam = new List<Player>();
            foreach (var player in _redTeam)
            {
                newGreenTeam.Add(player);
            }

            _redTeam.Clear();

            foreach (var player in _greenTeam)
            {
                _redTeam.Add(player);
            }

            _greenTeam.Clear();

            foreach (var player in newGreenTeam)
            {
                _greenTeam.Add(player);
            }

            grdSetTeamPositions.Rows.Clear();
            grdMatchingTeamPositions.Rows.Clear();

            foreach (var player in _redTeam)
            {
                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Tag });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = player.Number });

                grdSetTeamPositions.Rows.Add(row);
            }

            lblFavoured.Text = string.Empty;
        }

        private void grdPlaying_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            lblPlaying.Text = $"Playing ({grdPlaying.Rows.Count})";
        }

        private void grdPlaying_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            lblPlaying.Text = $"Playing ({grdPlaying.Rows.Count})";
        }

        private void grdSetTeamPositions_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                var playerId = Convert.ToInt32(grdSetTeamPositions[3, e.RowIndex].Value);
                var player = _Admin.GetPlayer(playerId);

                var position = grdSetTeamPositions[e.ColumnIndex, e.RowIndex]?.Value?.ToString();
                var mvp = string.Empty;

                if (position != null)
                {
                    switch (position.ToLower())
                    {
                        case "c":
                            mvp = player.CommanderRank.ToString();
                            break;

                        case "h":
                            mvp = player.HeavyRank.ToString();
                            break;

                        case "s":
                            mvp = player.ScoutRank.ToString();
                            break;

                        case "a":
                            mvp = player.AmmoRank.ToString();
                            break;

                        case "m":
                            mvp = player.MedicRank.ToString();
                            break;
                    }
                }

                grdSetTeamPositions[1, e.RowIndex].Value = mvp;
            }
        }

        private void rdoBalanceCC_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoBalanceCC.Checked)
            {
                CurrentBalanceMode = BalanceMode.SameClasses;
            }
        }

        private void rdoBalanceCH_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoBalanceCH.Checked)
            {
                CurrentBalanceMode = BalanceMode.OppositeClasses;
            }
        }

        private void rdoBalanceBoth_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoBalanceBoth.Checked)
            {
                CurrentBalanceMode = BalanceMode.Both;
            }
        }

        private void cmbPositionMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPositionMode.SelectedIndex == 0)
            {
                CurrentPositionMode = PositionMode.RedTeam;
            }
            else if (cmbPositionMode.SelectedIndex == 1)
            {
                CurrentPositionMode = PositionMode.GreenTeam;
            }
            else
            {
                CurrentPositionMode = PositionMode.Randomise;
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewNumber.Text))
            {
                MessageBox.Show("Please specify a member number");
            }

            if (string.IsNullOrEmpty(txtNewName.Text))
            {
                MessageBox.Show("Please specify a member name");
            }

            if (string.IsNullOrEmpty(txtNewTag.Text))
            {
                MessageBox.Show("Please specify a member tag");
            }

            if (_Admin.Players.Any(x => x.Number.ToString() == txtNewNumber.Text))
            {
                MessageBox.Show("A member with that number already exists");
            }

            var player = new Player()
            {
                Number = Convert.ToInt32(txtNewNumber.Text),
                RealName = txtNewName.Text,
                Tag = txtNewTag.Text,
                OverallRank = string.IsNullOrEmpty(txtNewOverall.Text) ? 0 : Convert.ToDecimal(txtNewOverall.Text),
                CommanderRank = string.IsNullOrEmpty(txtNewCommander.Text) ? 0 : Convert.ToDecimal(txtNewCommander.Text),
                HeavyRank = string.IsNullOrEmpty(txtNewHeavy.Text) ? 0 : Convert.ToDecimal(txtNewHeavy.Text),
                ScoutRank = string.IsNullOrEmpty(txtNewScout.Text) ? 0 : Convert.ToDecimal(txtNewScout.Text),
                AmmoRank = string.IsNullOrEmpty(txtNewAmmo.Text) ? 0 : Convert.ToDecimal(txtNewAmmo.Text),
                MedicRank = string.IsNullOrEmpty(txtNewMedic.Text) ? 0 : Convert.ToDecimal(txtNewMedic.Text)
            };

            JsonWriter.WritePlayerFile(player);

            _Admin.Players.Add(player);

            LoadPlayersIntoTable();
        }

        private void cmbUpdatePlayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            var playerId = (cmbUpdatePlayer.SelectedItem as UpdatePlayerItem).Value;

            var player = _Admin.GetPlayer(Convert.ToInt32(playerId));

            txtUpdateOverall.Text = player.OverallRank.ToString();
            txtUpdateCommander.Text = player.CommanderRank.ToString();
            txtUpdateHeavy.Text = player.HeavyRank.ToString();
            txtUpdateScout.Text = player.ScoutRank.ToString();
            txtUpdateAmmo.Text = player.AmmoRank.ToString();
            txtUpdateMedic.Text = player.MedicRank.ToString();
        }

        private void btnUpdatePlayer_Click(object sender, EventArgs e)
        {
            var playerId = (cmbUpdatePlayer.SelectedItem as UpdatePlayerItem).Value;

            var player = _Admin.GetPlayer(Convert.ToInt32(playerId));

            player.OverallRank = Convert.ToDecimal(txtUpdateOverall.Text);
            player.CommanderRank = Convert.ToDecimal(txtUpdateCommander.Text);
            player.HeavyRank = Convert.ToDecimal(txtUpdateHeavy.Text);
            player.ScoutRank = Convert.ToDecimal(txtUpdateScout.Text);
            player.AmmoRank = Convert.ToDecimal(txtUpdateAmmo.Text);
            player.MedicRank = Convert.ToDecimal(txtUpdateMedic.Text);
        }

        #endregion Event Handlers

        public enum AdminMode
        {
            None,
            Search,
            Edit,
            New
        }

        public enum PositionMode
        {
            RedTeam,
            GreenTeam,
            Randomise
        }

        public enum BalanceMode
        {
            SameClasses,
            OppositeClasses,
            Both
        }
    }
}
