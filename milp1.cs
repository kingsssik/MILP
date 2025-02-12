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
            int n = 4; //job 수
            int m = 3; //machine 수

            double M = 9999999;

            int[] r = new int[] { 1, 3, 2, 2, 3 };
            int[] d = new int[] { 20, 15, 25, 30 };

            Cplex cplex = new Cplex();
            Random rnd = new Random();

            //decision value

            IIntVar[][][][][] X_jlhzi = new IIntVar[n][][][][];
            for(int j = 0; j < n; j++)
            {
                X_jlhzi[j] = new IIntVar[r[j+1]][][][];
                for(int l = 0; l < r[j+1]; l++)
                {
                    X_jlhzi[j][l] = new IIntVar[n+1][][];
                    for(int h = 0; h < n+1; h++)
                    {
                        X_jlhzi[j][l][h] = new IIntVar[r[h]][];
                        for(int z = 0; z < r[h]; z++)
                        {
                            X_jlhzi[j][l][h][z] = new IIntVar[m];
                            for(int i = 0; i < m; i++)
                            {
                                X_jlhzi[j][l][h][z][i]= cplex.IntVar(0, 1, $"X_{j}_{l}_{h}_{z}_{i}");  
                            }
                        }
                    }
                }
            }


            INumVar[][] C_jl = new INumVar[n+1][];
            for(int h = 0; h < n+1; h++)
            {
                C_jl[h] = new INumVar[r[h]];
                for(int z = 0 ;z < r[h]; z++)
                {
                    C_jl[h][z] = cplex.NumVar(0.0, M, NumVarType.Float, $"C_{h}_{z}");
                }
            }

            INumVar[] T_j = new INumVar[n];
            for(int j = 0; j < n; j++)
            {
                T_j[j] = cplex.NumVar(0.0, M, NumVarType.Float, $"T_{j}");
            }


            //p_jli
            double[][][] p_jli = new double[n][][];
            for (int j = 0; j < n; j++)
            {
                p_jli[j] = new double[r[j+1]][];
                for(int l = 0; l < r[j+1]; l++)
                {
                    p_jli[j][l] = new double[m];
                    for(int i = 0; i < m; i++)
                    {
                        p_jli[j][l][i] = (Convert.ToDouble(rnd.Next(1000)) / 1000) * 10;
                    }
                }
            }


            //e_jli
            double[][][] e_jli = new double[n][][];
            for(int j = 0;j < n; j++)
            {
                e_jli[j] = new double[r[j+1]][];
                for(int l = 0;l < r[j+1]; l++)
                {
                    e_jli[j][l] = new double [m];
                    for(int i = 0;i < m; i++)
                    {
                        double a = (Convert.ToDouble(rnd.Next(1000)) / 1000) * 1;

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

            //S_jhi
            double[][][] S_jhi = new double[n][][];
            for(int j = 0; j < n; j++)
            {
                S_jhi[j] = new double[n+1][];
                for(int h = 0; h < n+1; h++)
                {
                    S_jhi[j][h] = new double [m];
                    for (int i = 0; i < m; i++)
                    {
                        S_jhi[j][h][i] = (Convert.ToDouble(rnd.Next(1000)) / 1000) * 6;
                    }
                    
                }
            }

            //objective
            INumExpr sumT = cplex.NumExpr();
            for (int j = 0; j < n; j++)
            {
                sumT = cplex.Sum(sumT, T_j[j]);
            }

            cplex.AddMinimize(sumT);

            //constraint

            //(1)
            for(int j = 0; j < n; j++)
            {
                for(int l = 0; l < r[j+1]; l++)
                {
                    IIntExpr sumX = cplex.IntExpr();
                    for(int h = 0; h < n+1; h++)
                    {
                        for(int z = 0; z < r[h]; z++)
                        {
                            for(int i = 0; i < m; i++)
                            {
                                sumX = cplex.Sum(sumX, X_jlhzi[j][l][h][z][i]);
                            }
                        }
                    }
                    cplex.AddEq(sumX, 1.0);
                }
            }


            //(2)
            for(int j = 0; j < n; j++)
            {
                for (int l = 0; l < r[j+1]; l++)
                {
                    for(int i = 0; i<m; i++)
                    {
                        INumExpr sumX_1 = cplex.NumExpr();
                        for(int h = 0; h < n+1; h++)
                        {
                            for(int z = 0; z < r[h]; z++)
                            {
                                sumX_1 = cplex.Sum(sumX_1, X_jlhzi[j][l][h][z][i]);
                            }
                        }
                        cplex.AddLe(sumX_1, e_jli[j][l][i]);
                    }
                }
            }

            //(3)
            for (int h = 1; h < n+1; h++)
            {
                for (int z = 0; z < r[h]; z++)
                {
                    INumExpr sumX_2 = cplex.NumExpr();
                    for (int j = 0; j < n; j++)
                    {
                        for (int l = 0; l < r[j+1]; l++)
                        {
                            for (int i = 0; i < m; i++) //오타? i=1 ---n까지??
                            {
                                sumX_2 = cplex.Sum(sumX_2, X_jlhzi[j][l][h][z][i]);
                            }
                        }
                    }
                    cplex.AddLe(sumX_2, 1.0);
                }
            }


            //(4)
            for (int i = 0; i < m; i++)
            {
                INumExpr sumX_3 = cplex.NumExpr();
                for (int j = 0; j < n; j++)
                {
                    for (int l = 0; l < r[j+1]; l++)
                    {
                        sumX_3 = cplex.Sum(sumX_3, X_jlhzi[j][l][0][0][i]);
                    }
                }
                cplex.AddLe(sumX_3, 1.0);
            }

            ////(5)
            //for (int h = 1; h < n + 2; h++) //h는 n? n+1? 까지
            //{
            //    for (int z = 0; z < r[h-1]; z++)
            //    {
            //        for (int i = 0; i < m; m++)
            //        {
            //            IIntExpr sumX_4 = cplex.IntExpr();

            //            for (int j = 0; j < n; j++)
            //            {
            //                for (int l = 0; l < r[j + 1]; l++)
            //                {
            //                    sumX_4 = cplex.Sum(sumX_4, X_jlhzi[j][l][h][z][i]);
            //                }
            //            }


            //            IIntExpr sumX_5 = cplex.IntExpr();

            //            for (int j = 0; j < n + 1; j++)
            //            {
            //                for (int l = 0; l < r[j]; l++)
            //                {
            //                    sumX_5 = cplex.Sum(sumX_5, X_jlhzi[h - 1][z][j][l][i]);
            //                }
            //            }

            //            cplex.AddLe(sumX_4, sumX_5);
            //        }
            //    }
            //}


            //(6)
            for (int j = 0; j < n; j++)
            {
                for(int l = 0;l < r[j+1]; l++)
                {
                    if (l == 0)
                    {
                        continue;
                    }
                    INumExpr sum_6 = cplex.NumExpr();
                    for(int h = 0; h < n + 1; h++)
                    {
                        for(int z =0;  z < r[h]; z++)
                        {
                            for(int i = 0; i < m; i++)
                            {
                                INumExpr term = cplex.Prod(X_jlhzi[j][l][h][z][i], (p_jli[j][l][i] + S_jhi[j][h][i]));
                                sum_6 = cplex.Sum(sum_6, term);
                            }
                        }
                    }
                    cplex.AddGe(C_jl[j][l], cplex.Sum(C_jl[j][l - 1], sum_6));
                }
            }

            //(7) ?????????
            for (int j = 0; j < n; j++)
            {
                for (int l = 0; l < r[j + 1]; l++)
                {
                    for (int h = 1; h < n + 1; h++)
                    {
                        for (int z = 0; z < r[h]; z++)
                        {
                            INumExpr sumX_7 = cplex.NumExpr();

                            INumExpr term = cplex.NumExpr();
                            for (int i = 0; i < m; i++)
                            {
                                sumX_7 = cplex.Sum(sumX_7, X_jlhzi[j][l][h][z][i]);


                                term = cplex.Prod(X_jlhzi[j][l][h][z][i], (p_jli[j][l][i] + S_jhi[j][h][i]));
                            }
                            INumExpr term_1 = cplex.Prod(M, cplex.Sum(1.0, cplex.Prod(-1.0, sumX_7)));


                            INumExpr left = cplex.Sum(C_jl[h][z], term, cplex.Prod(term_1, -1.0));
                        }
                    }

                }
            }

            //(8) t_j가 뭘의미?(total?)
            for (int j = 0; j < n; j++)
            {
                INumExpr sumC = cplex.NumExpr();
                for (int l = 0; l < r[j+1]; l++)
                {
                    cplex.AddGe(T_j[j], cplex.Sum(sumC, (-1.0) * d[j]));
                }
                
            }



            //(9)
            for (int j = 0; j < n; j++)
            {
                cplex.AddGe(T_j[j], 0.0);

                for (int l = 0; l < r[j+1]; l++)
                {
                    cplex.AddGe(C_jl[j][l], 0.0);
                }
            }



            //solve
            if (cplex.Solve())
            {
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine("Solution status = " + cplex.GetStatus());
                Console.WriteLine($"Objective Value (Sum of T_j) = {cplex.ObjValue}");
                Console.WriteLine("---------------------------------------------------");

                // 예시: T_j, C_j,l 출력
                for (int j = 0; j < n; j++)
                {
                    Console.WriteLine($"T_{j} = {cplex.GetValue(T_j[j])}");
                    for (int l = 0; l < r[j+1]; l++)
                    {
                        Console.WriteLine($"   C_{j},{l} = {cplex.GetValue(C_jl[j][l])}");
                    }
                }

                // 예시: X_jlhzi 중 1인 것만 출력
                // (실제로는 아주 많을 수 있으니, 필요한 것만 보거나 Debug 모드에서 확인 권장)
                for (int j = 0; j < n; j++)
                {
                    for (int l = 0; l < r[j+1]; l++)
                    {
                        for (int h = 0; h < n+1; h++)
                        {
                            for (int z = 0; z < r[h]; z++)
                            {
                                for (int i = 0; i < m; i++)
                                {
                                    double valX = cplex.GetValue(X_jlhzi[j][l][h][z][i]);
                                    if (valX > 0.5) // 1인 것으로 판단
                                    {
                                        Console.WriteLine(
                                            $"X_{j}_{l}_{h}_{z}_{i} = {valX}"
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No feasible solution found. Status = " + cplex.GetStatus());
            }






        }






        catch (ILOG.Concert.Exception exc)
        {
            System.Console.WriteLine("Concert exception '" + exc + "' caught");
        }

    }

}
