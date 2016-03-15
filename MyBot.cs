using System.Collections.Generic;
using Pirates;
using System;


namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        const string A = @"run.bat bots\csharp\ExampleBot\MyBot.cs bots\demoBot1.pyc";

        public int Moves(List<Pirate> list, Pirate p)
        {

            if (p.HasTreasure)
                return 1;

            else
            {
                if (Has3Trsure(list))
                {
                    return 3;
                }
                if (Has2Trsure(list))
                {
                    return 2;
                }

                return p.Id % 2 == 0 ? 2 : 1;
            }
        }

        public bool Has3Trsure(List<Pirate> P)
        {
            bool a = false, b = false, c = false;
            for (int i = 0; i < P.Count; i++)
            {
                if (!a)
                    a = P[i].HasTreasure;
                else
                {
                    if (!b)
                        b = P[i].HasTreasure;
                    else
                    {
                        if (!c)
                            c = P[i].HasTreasure;
                    }
                }
            }
            return a && b && c;

        }
        public bool Has2Trsure(List<Pirate> P)
        {
            bool a = false, b = false;
            for (int i = 0; i < P.Count; i++)
            {
                if (!a)
                    a = P[i].HasTreasure;
                else
                {
                    if (!b)
                        b = P[i].HasTreasure;

                }
            }
            return a && b;

        }

        ////////////////////////////////////////////////////////////////////////////////////////////


        public void DoTurn(IPirateGame game)
        {
            List<Pirate> MyPirate = game.MyPirates();
            List<Pirate> EnemyPirate = game.EnemyPirates();
            List<Treasure> MyTresure = game.Treasures();



        }


    }




    //מחלק לקבוצות , בודק מרחק מאוצרות, מראה מרחק הכי קטן


    class TresureDistance
    {
        public List<Treasure> Tresures = new List<Treasure>();
        IPirateGame game { get; set; }
        public TresureDistance(List<Treasure> t,IPirateGame Game)
        {
            this.Tresures = t;
            this.game = Game;
        }



        public Treasure GetClosest(Pirate pirate)
        {
            List<int> Dis = new List<int>();
            foreach (Treasure t in Tresures)
            {
                Dis.Add(game.Distance(pirate.Location, t.Location));
            }



            int index = 0;
            int Min = Dis[0];
            for (int i = 1; i < Dis.Count; i++)
            {
                if (Min > Dis[i])
                {
                    Min = Dis[i];
                    index = i;
                }
            }
            return Tresures[index];

        }
    }

}







