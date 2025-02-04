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


            int[] r = new int[] { 3, 2, 2, 3 }; //
            int[] d = new int[] { 20, 15, 25, 30 }; //

            Cplex cplex = new Cplex();
            Random ran = new Random();




            //decision variable

            IIntVar[][][][][] X_jlhzi = new IIntVar[n][][][][];
            for(int j = 0;  j < n; j++)
            {
                X_jlhzi[j] = new IIntVar[r[j]][][][];
                for(int l = 0; l < r[j]; l++)
                {
                    X_jlhzi[j][l] = new IIntVar[n+1][][];
                    for(int h =0; h < n+1; h++)
                    {
                        X_jlhzi[j][l][h] = new IIntVar[r[j]][];
                        for(int z = 0; z < r[j]; z++)
                        {
                            X_jlhzi[j][l][h][z] = new IIntVar[m];
                        }
                    }
                }
            }



            IIntVar[][] C_jl = new IIntVar[n][];
            for(int j = 0; j < n; j++)
            {
                C_jl[j] = new IIntVar[r[j]];
            }



            IIntVar[] T_j = new IIntVar[n];


            //intialization

            double[][][] p_jli = new double[n][][];
            for (int j = 0; j < n; j++)
            {
                p_jli[j] = new double[r[j]][];
                for (int l = 0; l < r[j]; l++)
                {
                    p_jli[j][l] = new double[m];
                    for (int i = 0; i < m; i++)
                    {
                        p_jli[j][l][i] = (Convert.ToDouble(ran.Next(1000)) / 1000) * 10;
                    }
                }
            }



            double[][][] e_jli = new double[n][][];
            for (int j = 0; j < n; j++)
            {
                e_jli[j] = new double[r[j]][];
                for (int l = 0; l < r[j]; l++)
                {
                    e_jli[j][l] = new double[m];
                    for (int i = 0; i < m; i++)
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

            double[][][] S_jhi = new double[n][][];
            for(int j = 0; j < n; j++)
            {
                S_jhi[j] = new double[n+1][];
                for (int h = 0; h < n+1; h++)
                {
                    S_jhi[j][h] = new double[m];
                    for(int i = 0;i < m; i++)
                    {
                        S_jhi[j][h][i] = (Convert.ToDouble(ran.Next(1000)) / 1000) * 6;
                    }
                }
            }


            //constraint

            //(1)
           
            for (int j = 0;j < n; j++)
            {
                for(int l = 0; l < r[j]; l++)
                {
                    INumExpr sumX = cplex.NumExpr();
                    for (int h = 0; h < n+1; h++)
                    {
                        for(int z = 0; z < m; z++)
                        {
                            for(int i = 0; i < m; i++)
                            {
                                sumX = cplex.Sum(sumX, X_jlhzi[j][l][h][z][i]);
                            }
                        }
                    }
                }
            }

 




        }






        catch (ILOG.Concert.Exception exc)
        {
            System.Console.WriteLine("Concert exception '" + exc + "' caught");
        }

    }
}
