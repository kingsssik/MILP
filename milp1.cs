using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using ILOG.Concert;
using ILOG.CPLEX;


class Milp
{
    static void Main(string[] args)
    {
        try
        {
            int n = 4; // job 수
            int m = 3; // machine 수

            double M = 999999; //큰 양수


            int[] r = new int[] { 0, 3, 2, 2, 3 }; //
            int[] d = new int[] { 20, 15, 25, 30 }; //

            Cplex cplex = new Cplex();
            Random ran = new Random();




            //decision variable

            IIntVar[][][][][] X_jlhzi = new IIntVar[n + 1][][][][];
            for (int j = 1; j < n + 1; j++)
            {
                X_jlhzi[j] = new IIntVar[r[j] + 1][][][];
                for (int l = 1; l < r[j] + 1; l++)
                {
                    X_jlhzi[j][l] = new IIntVar[n + 1][][];
                    for (int h = 0; h < n + 1; h++)
                    {
                        X_jlhzi[j][l][h] = new IIntVar[r[j] + 1][];
                        for (int z = 1; z < r[j] + 1; z++)
                        {
                            X_jlhzi[j][l][h][z] = new IIntVar[m + 1];
                            for (int i = 0; i < m + 1; i++)
                            {
                                X_jlhzi[j][l][h][z][i] = cplex.IntVar(0, 1, $"X_{j}_{l}_{h}_{z}_{i}");
                            }
                        }
                    }
                }
            }



            IIntVar[][] C_jl = new IIntVar[n + 1][];
            for (int j = 1; j < n + 1; j++)
            {
                C_jl[j] = new IIntVar[r[j] + 1];
            }



            IIntVar[] T_j = new IIntVar[n+1];


            //intialization

            double[][][] p_jli = new double[n + 1][][];
            for (int j = 1; j < n + 1; j++)
            {
                p_jli[j] = new double[r[j] + 1][];
                for (int l = 1; l < r[j] + 1; l++)
                {
                    p_jli[j][l] = new double[m + 1];
                    for (int i = 1; i < m + 1; i++)
                    {
                        p_jli[j][l][i] = (Convert.ToDouble(ran.Next(1000)) / 1000) * 10;
                    }
                }
            }



            double[][][] e_jli = new double[n + 1][][];
            for (int j = 1; j < n + 1; j++)
            {
                e_jli[j] = new double[r[j] + 1][];
                for (int l = 1; l < r[j] + 1; l++)
                {
                    e_jli[j][l] = new double[m + 1];
                    for (int i = 1; i < m + 1; i++)
                    {
                        double a = (Convert.ToDouble(ran.Next(1000)) / 1000) * 1;

                        if (a >= 0.3)
                        {
                            e_jli[j][l][i] = 1;
                        }
                        else
                        {
                            e_jli[j][l][i] = 0;
                        }

                    }
                }
            }

            double[][][] S_jhi = new double[n + 1][][];
            for (int j = 1; j < n + 1; j++)
            {
                S_jhi[j] = new double[n + 1][];
                for (int h = 0; h < n + 1; h++)
                {
                    S_jhi[j][h] = new double[m + 1];
                    for (int i = 1; i < m + 1; i++)
                    {
                        S_jhi[j][h][i] = (Convert.ToDouble(ran.Next(1000)) / 1000) * 6;
                    }
                }
            }

            //objective
            INumExpr sumT = cplex.NumExpr();
            for (int j = 1; j < n+1; j++)
            {
                sumT = cplex.Sum(sumT, T_j[j]);
            }
            //cplex.AddMinimize(sumT);

            //constraint

            //(1)

            for (int j = 1; j < n + 1; j++)
            {
                for (int l = 1; l < r[j] + 1; l++)
                {
                    INumExpr sumX = cplex.NumExpr();
                    for (int h = 0; h < n + 1; h++)
                    {
                        for (int z = 1; z < r[h] + 1; z++)
                        {
                            for (int i = 1; i < m + 1; i++)
                            {
                                sumX = cplex.Sum(sumX, X_jlhzi[j][l][h][z][i]);
                            }
                        }
                    }
                    cplex.AddEq(sumX, 1.0);
                }
            }



            //(2)

            for (int j = 1; j < n + 1; j++)
            {
                for (int l = 1; l < r[j] + 1; l++)
                {
                    for (int i = 1; i < m + 1; i++)
                    {
                        INumExpr sumX_1 = cplex.NumExpr();

                        for (int h = 0; h < n + 1; h++)
                        {
                            for (int z = 1; z < r[h] + 1; z++)
                            {
                                sumX_1 = cplex.Sum(sumX_1, X_jlhzi[j][l][h][z][i]);
                            }
                        }
                        cplex.AddLe(sumX_1, e_jli[j][l][i]);
                    }
                }
            }

            //(3)

            for (int h = 1; h < n + 1; h++)
            { 
                for (int z = 1; z < r[h]+1; z++)
                {
                    INumExpr sumX_2 = cplex.NumExpr();
                    for (int j = 1; j <n+1; j++)
                    {
                        for(int l =1; l < r[j]+1; l++)
                        {
                            for(int i = 1; i<n+1; i++)
                            {
                                sumX_2 = cplex.Sum(sumX_2,X_jlhzi[j][l][h][z][i]);
                            }
                        }
                    }
                    cplex.AddLe(sumX_2, 1.0);
                }
            }

            //(4)

            for(int i = 0; i<m+1; i++)
            {
                INumExpr sumX_3 = cplex.NumExpr();
                for(int j = 1;j <n+1; j++)
                {
                    for(int l =1; l < r[j]+1; l++)
                    {
                        sumX_3 = cplex.Sum(sumX_3, X_jlhzi[j][l][0][1][i]);
                    }
                }
                cplex.AddLe(sumX_3,1.0);
            }

            //(5)








        }
        catch (ILOG.Concert.Exception exc)
        {
            System.Console.WriteLine("Concert exception '" + exc + "' caught");
        }

    }
}
