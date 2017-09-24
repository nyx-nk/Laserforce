using System;
using Analysis.Models;

namespace Analysis
{
    public class MathFunctions
    {
        public static LinearRegressionEquation LinearRegression(double[] xValues, double[] yValues, int start, int end)
        {
            var equation = new LinearRegressionEquation();

            double totalCodeviates = 0;
            double totalX = 0;
            double totalY = 0;
            double totalXSquared = 0;
            double totalYSquared = 0;
            double ssX = 0;
            double ssY = 0;
            double count = end - start;
            double sCo = 0;

            for (int index = start; index < end; ++index)
            {
                double x = xValues[index];
                double y = yValues[index];

                totalCodeviates += x * y;
                totalX += x;
                totalY += y;
                totalXSquared += x * x;
                totalYSquared += y * y;
            }

            ssX = totalXSquared - ((totalX * totalX) / count);
            ssY = totalYSquared - ((totalY * totalY) / count);
            double rNumerator = (count * totalCodeviates) - (totalX * totalY);
            double rDenominator = (count * totalXSquared - (totalX * totalX)) * (count * totalYSquared - (totalY * totalY));
            sCo = totalCodeviates - ((totalX * totalY) / count);

            double meanX = totalX / count;
            double meanY = totalY / count;
            double r = rNumerator / Math.Sqrt(rDenominator);

            equation.m = sCo / ssX;
            equation.c = meanY - ((sCo / ssX) * meanX);
            equation.rSquared = (r * r);

            if (double.IsNaN(equation.m)) equation.m = 0;
            if (double.IsNaN(equation.c)) equation.c = 0;
            if (double.IsNaN(equation.rSquared)) equation.rSquared = 0;

            return equation;
        }

        public static MultivariateRegressionEquation MultivariateLinearRegression(double[][] data)
        {
            var designMatrix = CreateDesignMatrix(data);

            var result = new MultivariateRegressionEquation();
            result.Coefficients = SolveDesignMatrixForCoefficients(designMatrix);
            result.rSquared = CalculateRSquaredForMatrix(data, result.Coefficients);

            return result;
        }

        public static double PredictValueFromData(double[][] data, params double[] values)
        {
            var designMatrix = CreateDesignMatrix(data);

            var coefficients = SolveDesignMatrixForCoefficients(designMatrix);

            var result = coefficients[0];

            for (int i = 0; i < values.Length; ++i)
            {
                result += values[i] * coefficients[i + 1];
            }

            return result;
        }

        private static double[][] CreateDesignMatrix(double[][] data)
        {
            int rows = data.Length;
            int cols = data[0].Length;
            var result = CreateBlankMatrix(rows, cols + 1);
            for (int i = 0; i < rows; ++i)
            {
                result[i][0] = 1.0;
            }

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    result[i][j + 1] = data[i][j];
                }
            }

            return result;
        }

        private static double[] SolveDesignMatrixForCoefficients(double[][] designMatrix)
        {
            var rows = designMatrix.Length;
            var cols = designMatrix[0].Length;
            var matrixX = CreateBlankMatrix(rows, cols - 1);
            var matrixY = CreateBlankMatrix(rows, 1);

            int j;
            for (int i = 0; i < rows; ++i)
            {
                for (j = 0; j < cols - 1; ++j)
                {
                    matrixX[i][j] = designMatrix[i][j];
                }

                matrixY[i][0] = designMatrix[i][j];
            }

            var transposedMatrixX = TransposeMatrix(matrixX);
            var multipliedMatrixX = MultiplyMatrices(transposedMatrixX, matrixX);
            var inverseMatrix = InverseMatrix(multipliedMatrixX);
            var multipliedInverseMatrix = MultiplyMatrices(inverseMatrix, transposedMatrixX);

            var resultingMultipledMatrix = MultiplyMatrices(multipliedInverseMatrix, matrixY);
            var result = ConvertMatrixToVector(resultingMultipledMatrix);

            return result;
        }

        private static double[][] CreateBlankMatrix(int rows, int cols)
        {
            var result = new double[rows][];
            for (int i = 0; i < rows; ++i)
            {
                result[i] = new double[cols];
            }

            return result;
        }

        private static double[][] DuplicateMatrix(double[][] matrix)
        {
            var result = CreateBlankMatrix(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i)
            {
                for (int j = 0; j < matrix[i].Length; ++j)
                {
                    result[i][j] = matrix[i][j];
                }
            }

            return result;
        }

        private static double[][] TransposeMatrix(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            var result = CreateBlankMatrix(cols, rows);
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    result[j][i] = matrix[i][j];
                }
            }

            return result;
        }

        private static double[][] MultiplyMatrices(double[][] matrixA, double[][] matrixB)
        {
            var rowsA = matrixA.Length;
            var colsA = matrixA[0].Length;

            var rowsB = matrixB.Length;
            var colsB = matrixB[0].Length;

            if (colsA != rowsB) throw new InvalidOperationException("Matrices are not compatible for multiplication.");

            var result = CreateBlankMatrix(rowsA, colsB);

            for (int i = 0; i < rowsA; ++i)
            {
                for (int j = 0; j < colsB; ++j)
                {
                    for (int k = 0; k < colsA; ++k)
                    {
                        result[i][j] += matrixA[i][k] * matrixB[k][j];
                    }
                }
            }

            return result;
        }

        private static double[][] InverseMatrix(double[][] matrix)
        {
            var length = matrix.Length;
            var result = DuplicateMatrix(matrix);

            int[] perm;
            int toggle;
            var lum = DecomposeMatrix(matrix, out perm, out toggle);
            if (lum == null) throw new InvalidOperationException("The matrix could not be decomposed.");

            var b = new double[length];
            for (int i = 0; i < length; ++i)
            {
                for (int j = 0; j < length; ++j)
                {
                    b[j] = i == perm[j] ? 1.0 : 0.0;
                }

                var x = SolveDecomposedMatrix(lum, b);

                for (int j = 0; j < length; ++j)
                {
                    result[j][i] = x[j];
                }
            }

            return result;
        }

        private static double[][] DecomposeMatrix(double[][] matrix, out int[] perm, out int toggle)
        {
            var rows = matrix.Length;
            var cols = matrix[0].Length;
            if (rows != cols) throw new InvalidOperationException("Matrix is not square.");

            var result = DuplicateMatrix(matrix);

            perm = new int[rows];
            for (int i = 0; i < rows; ++i)
            {
                perm[i] = i;
            }

            toggle = 1;

            for (int j = 0; j < rows - 1; ++j)
            {
                var colMax = Math.Abs(result[j][j]);
                int pRow = j;

                for (int i = j + 1; i < rows; ++i)
                {
                    if (Math.Abs(result[i][j]) > colMax)
                    {
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }

                if (pRow != j)
                {
                    var rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int temp = perm[pRow];
                    perm[pRow] = perm[j];
                    perm[j] = temp;

                    toggle = -toggle;
                }

                if (result[j][j] == 0.0)
                {
                    int goodRow = -1;
                    for (int row = j + 1; row < rows; ++row)
                    {
                        if (result[row][j] != 0.0)
                        {
                            goodRow = row;
                        }
                    }

                    if (goodRow == -1) throw new InvalidOperationException("Doolittle's Decomposition method does not work.");

                    var rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int temp = perm[goodRow];
                    perm[goodRow] = perm[j];
                    perm[j] = temp;

                    toggle = -toggle;
                }

                for (int i = j + 1; i < rows; ++i)
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < rows; ++k)
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }
            }

            return result;
        }

        private static double[] SolveDecomposedMatrix(double[][] matrix, double[] b)
        {
            var length = matrix.Length;
            var x = new double[length];
            b.CopyTo(x, 0);

            for (int i = 1; i < length; ++i)
            {
                var sum = x[i];
                for (int j = 0; j < i; ++j)
                {
                    sum -= matrix[i][j] * x[j];
                }

                x[i] = sum;
            }

            x[length - 1] /= matrix[length - 1][length - 1];

            for (int i = length - 2; i >= 0; --i)
            {
                var sum = x[i];
                for (int j = i + 1; j < length; ++j)
                {
                    sum -= matrix[i][j] * x[j];
                }
                x[i] = sum / matrix[i][i];
            }

            return x;
        }

        private static double[] ConvertMatrixToVector(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (cols != 1) throw new InvalidOperationException("Matrix is not one-dimensional.");

            var result = new double[rows];
            for (int i = 0; i < rows; ++i)
            {
                result[i] = matrix[i][0];
            }

            return result;
        }

        private static double CalculateRSquaredForMatrix(double[][] data, double[] coefficients)
        {
            var rows = data.Length;
            var cols = data[0].Length;

            double sumY = 0.0;
            for (int i = 0; i < rows; ++i)
            {
                sumY += data[i][cols - 1];
            }

            double meanY = sumY / rows;

            double sumSquaredResidual = 0.0;
            double sumSquareTotal = 0.0;
            double y;
            double predictedY;
            for (int i = 0; i < rows; ++i)
            {
                y = data[i][cols - 1];

                predictedY = coefficients[0];
                for (int j = 0; j < cols - 1; ++j)
                {
                    predictedY += coefficients[j + 1] * data[i][j];
                }

                sumSquaredResidual += (y - predictedY) * (y - predictedY);
                sumSquareTotal += (y - meanY) * (y - meanY);
            }

            double rSquared = 0.0;

            if (sumSquareTotal != 0)
            {
                rSquared = 1.0 - (sumSquaredResidual / sumSquareTotal);
            }

            return rSquared;
        }
    }
}