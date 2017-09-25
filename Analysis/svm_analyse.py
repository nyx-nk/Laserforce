"""
Laserforce Space Marines 5
Fancy Machine Learning-based Stat Miner
Version: 1.0 (23/09/2017)
Author: Andrew Chen (andrewc6126@gmail.com)

- Uses a SVM to predict if a team will win or lose if a certain class has certain stats
- Does not model the team effects (i.e. what happens when two particular players are together in particular positions)
- Does not model team size effects (i.e. what happens if the teams are different sizes, or there are more or fewer players)
- Does not model the opponent effects (i.e. what happens when a particular team plays against a particular other team)
- Does not model intangibles (duh, because we don't have stats for that)

Dependencies:
- scikit-learn module (called sklearn below because imported Python modules can't have - in their names)
"""

import sqlite3
from sklearn import svm

DB_STRING = "AKLStats/local_stats.db"

#Read or write to/from the database
def accessDB(command, args=None):
    conn = sqlite3.connect(DB_STRING) #Connect to the DB
    c = conn.cursor() #Initialise the cursor
    #Execute the command, if there are args (or not)
    #Doing it this way in theory catches SQL injections...
    if (args):
        c.execute(command, args)
    else:
        c.execute(command)

    output = [i for i in c] #Form the response
    conn.commit() #Commit (save) and close the connection
    c.close()
    return output

def main():
    ###Extract game data from db and move it into labelled dictionaries
    games = {}
    db_games = accessDB("SELECT * FROM Game")
    for game in db_games:
        games[game[0]] = {
            "red_score": game[1] + game[2],
            "green_score": game[3] + game[4],
            #"winner": 0 if (game[1] + game[2]) > (game[3] + game[4]) else 1,
            "winner": game[5], #Just use the text so that it can be compared against later on
            "red_elim": game[6],
            "green_elim": game[7]
        }

    ###Extract player data from db, keeping only the relevant stats, by class
    players = {"Commander": {}, "Heavy Weapons": {}, "Scout": {}, "Ammo Carrier": {}, "Medic": {}}
    db_players = accessDB("SELECT * FROM PlayerGameScore")
    for player in db_players:
        #In the players dictionary, in the appropriate class subdictionary, where the specific instance ID is the key
        #Extract stats that are common to all classes
        players[player[3]][player[0]] = {
            "player_name": player[1],
            "win" : 1 if (games[player[-2]]["winner"] == player[2]) else 0, #1 = win, 0 = loss
            "shots_hit": player[4],
            "shots_fired": player[5],
            "times_zapped": player[6],
            "times_missiled": player[7],
            "medic_hits": player[12],
            "own_medic_hits": player[13],
            "lives_left": player[18],
            #"score": player[19],
            "shot_opponents": player[25],
            "shot_team": player[26],
            #"rank": player[30],
            "bases_destroyed": player[31],
            "accuracy": player[32]}
            #own_nuke_cancels has been left out because there's only one in our entire dataset: Lyte as a Ammo hitting DK as Commander

        #Extract class-specific stats
        if (player[3] == "Commander"):
            players[player[3]][player[0]].update({        
                "nukes_activated": player[9],
                "nukes_detonated": player[10],
                "nukes_cancelled": player[11],
                "missile_hits": player[8],
                "missiled_opponent": player[27],
                "missiled_team": player[28]})
        if (player[3] == "Heavy Weapons"):
            players[player[3]][player[0]].update({
                "missile_hits": player[8],
                "missiled_opponent": player[27],
                "missiled_team": player[28]})
        if (player[3] == "Scout"):
            players[player[3]][player[0]].update({
                "shot_three_hit": player[21],
                "rapid_fires": player[15]})
        if (player[3] == "Ammo Carrier"):
            players[player[3]][player[0]].update({
                "ammo_boosts": player[17],
                "resupplies": player[29]})
        if (player[3] == "Medic"):
            players[player[3]][player[0]].update({
                "lives_boosts": player[16],
                "resupplies": player[29]})

    ###Run the analysis for each class separately

    #x is the input data, one list per instance
    #y is the classification labels for the input data where 1 = win, 0 = loss
    #x_labels are the factor names for processing afterwards
    dataset = {i: {"x":[], "y":[], "x_labels":[]} for i in players}

    for cl in players: #class is a reserved keyword in Python, so "cl" will have to do
        print cl
        dataset[cl]["x_labels"] = players[cl][players[cl].keys()[0]].keys() #Get the keys from the first player in this class
        #Remove the "player_name" and "win" labels
        dataset[cl]["x_labels"] = [label for label in dataset[cl]["x_labels"] if label not in ["player_name", "win"]]
        for pl in players[cl]:
            #Form a data vector with all of the values in the same order as the labels 
            data_vector = [players[cl][pl][label] for label in dataset[cl]["x_labels"]]
            dataset[cl]["x"].append(data_vector)
            dataset[cl]["y"].append(players[cl][pl]["win"]) #The classification is either a win (1) or a loss (0)

        model = svm.SVC(kernel="linear") #Initialise Support Vector Machine
        model.fit(dataset[cl]["x"], dataset[cl]["y"]) #Fit the SVM

        #List the factors by their relative importance in determining win/loss
        factor_list = []
        for factor in range(len(dataset[cl]["x_labels"])):
            #Each element is (Factor Name, Magnitude/Importance))
            factor_list.append((dataset[cl]["x_labels"][factor], model.coef_[0][factor]))
        factor_list.sort(key=lambda x:abs(x[1]), reverse=True) #Sort by the importance (magnitude only), not by the factor names

        ###Print out the results
        for f in factor_list:
            print f[0],":", f[1]
        print "" #Just to make things look slightly tidier

###END, run the code already
main()