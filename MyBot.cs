using System.Collections.Generic;
using Pirates;
using System;


namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {

        /* To Add 
                              Done מניעת התנגשיות
                               אסטרגית תקיפה    
        */

        const string A = @"run.bat bots\csharp\ExampleBot\MyBot.cs bots\demoBot1.py";




        bool Has3 = false;

        public int Moves(List<Pirate> list, Pirate p)
        {

            if (p.HasTreasure)
                return 1;

            else
            {
                if (Has3Trsure(list) && list.Count == 4)
                {
                    return 3;
                }
                if (Has2Trsure(list))
                {
                    if (list.Count == 4)
                        return 2;
                    if (list.Count == 3)
                        return 4;
                }
                if (list.Count == 4)
                    return p.Id % 2 == 0 ? 2 : 1;

                if (HasOneTrsure(list, p) && !Has3)
                {
                    Has3 = true;
                    return 3;
                }
                if (HasOneTrsure(list, p) && Has3)
                {
                    return 2;
                }
                if (!Has3)
                {
                    Has3 = true;
                    return 3;
                }
                return 2;

            }
        }

        private bool HasOneTrsure(List<Pirate> list, Pirate My) //Option 1 DoTurn7
        {

            foreach (Pirate p in list)
            {
                if (p != My && p.HasTreasure)
                    return true;
            }
            return false;

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






        public void DoTurn7(IPirateGame game)
        {
            /* Option 1
            game.Debug("DoTurn7!!!!");
            CA.PlayIn7 = true;
            List<Pirate> MyPirate = game.MyPirates();
            MyPirate.RemoveAt(3);
            List<Pirate> EnemyPirate = game.EnemyPirates();
            List<Treasure> MyTresure = game.Treasures();
            TresureDistance TD = new TresureDistance(MyTresure, game);

            // Option 1
            for (int i = 0; i < 3; i++)
            {
                Location Des;
                if (MyPirate[i].HasTreasure)
                    Des = MyPirate[i].InitialLocation;
                else {
                    Treasure GoToTresure = TD.GetClosest(MyPirate[i]);
                    Des = GoToTresure.Location;
                }


                List<Location> possibleLocations = game.GetSailOptions(MyPirate[i], Des, Moves(MyPirate, MyPirate[i]));
                Location locationToGo = CA.BestLoction(possibleLocations);

                game.SetSail(MyPirate[i], locationToGo);
            }
            Has3 = false;
            */
            // Option2 


        /*    game.Debug("DoTurn7!!!! -- Option 2");
            CA.PlayIn7 = true;
            List<Pirate> MyPirate = game.MyPirates();
            MyPirate.RemoveAt(3);
            List<Pirate> EnemyPirate = game.EnemyPirates();
            List<Treasure> MyTresure = game.Treasures();
            TresureDistance TD = new TresureDistance(MyTresure, game);

            // Option 1
            for (int i = 0; i < 3; i++)
            {
                Location Des;
                if (MyPirate[i].HasTreasure)
                    Des = MyPirate[i].InitialLocation;
                else {
                    Treasure GoToTresure = TD.GetClosest(MyPirate[i]);
                    Des = GoToTresure.Location;
                }

                List<Location> possibleLocations = game.GetSailOptions(MyPirate[i], Des, Moves(MyPirate, MyPirate[i]));
                Location locationToGo = CA.BestLoction(possibleLocations);

                game.SetSail(MyPirate[i], locationToGo);

            */
            }
        







        ColltionAvoider CA = new ColltionAvoider();
        public Location[] Des = new Location[4];

        public void DoTurn(IPirateGame game)
        {

            if (game.Treasures().Count != 0)
            {
                MovesOptions mp = new MovesOptions();
                mp.game = game;
                List<Pirate> MyPirate = game.MyPirates();
                List<Pirate> EnemyPirate = game.EnemyPirates();
                List<Treasure> MyTresure = game.Treasures();
                TresureDistance TD = new TresureDistance(MyTresure, game);


                /*  if (game.GetMyScore() >= 7 && !MyPirate[3].HasTreasure)
                  {
                      DoTurn7(game);
                  }
                  else
                  { 
                  */

                //////////////////////////////////////////////////////////////////////////////////////////////////////////



                int index = 0;

                foreach (Pirate pirate in MyPirate)
                {

                    game.Debug("Pirate " + pirate.Id);


                    if (pirate.HasTreasure)
                    {
                        game.Debug("Pirate has Tresure");
                        Des[index] = pirate.InitialLocation;
                        mp.list.Add(new PirateWithSelectedTresures(pirate, Des[index]));
                    }

                    else {
                        game.Debug("Pirate dosent have Tresure");

                        // || ChackIfTresueExistsAt(Des[index],game) == false
                        if (Des[index] == null || (Des[index].Col == pirate.InitialLocation.Col && Des[index].Row == pirate.InitialLocation.Row))
                        {
                            Treasure GoToTresure = TD.GetClosest(pirate);

                            //   game.Debug(String.Format("Pirate Number {0} Going To Tresure At {1} : {2} ", pirate.Id, GoToTresure.Location.Col, GoToTresure.Location.Row));
                            Des[index] = GoToTresure.Location;

                        }
                        if (ChackIfTresueExistsAt(Des[index], game))
                        {

                            game.Debug("Adding new Destination");
                            mp.list.Add(new PirateWithSelectedTresures(pirate, Des[index]));
                            game.Debug(String.Format("Pirate Number {0} Going To Tresure At {1} : {2} ", pirate.Id, Des[index].Col, Des[index].Row));
                            game.Debug("Destination Added");
                        }
                        else
                        {

                            Treasure GoToTresure = TD.GetClosest(pirate);
                            game.Debug(String.Format("Pirate Number {0} Going To Tresure At {1} : {2} ", pirate.Id, GoToTresure.Location.Col, GoToTresure.Location.Row));
                            Des[index] = GoToTresure.Location;
                            mp.list.Add(new PirateWithSelectedTresures(pirate, Des[index]));
                        }
                    }
                    index++;
                }



                int[] A = mp.RegularGameMoves();

                game.Debug(" **************************************** A --- {0} ,{1} , {2} , {3} ************************************* ", A[0], A[1], A[2], A[3]);



                for (int i = 0; i < 4; i++)
                {
                    game.Debug("Looking for Posiable locations");
                    List<Location> possibleLocations = game.GetSailOptions(mp.list[i].pirate, Des[i], A[i]);
                    game.Debug("Checking Best Location");
                    Location locationToGo = CA.BestLoction(possibleLocations);
                    game.Debug("Found Best Location");
                    game.Debug("Pirate Number {0} Go To Location {1} :{2}", mp.list[i].pirate.Id, locationToGo.Col, locationToGo.Row);
                    if (locationToGo.Col != mp.list[i].pirate.Location.Col || (locationToGo.Row != mp.list[i].pirate.Location.Row))
                        game.SetSail(mp.list[i].pirate, possibleLocations[0]);
                }

            }
            else
            {
                foreach (Pirate p in game.MyPirates())
                {
                    Location des = p.InitialLocation;
                    int moves = 1;
                    List<Location> options = game.GetSailOptions(p, des, 1);
                    game.SetSail(p, options[0]);
                }
            }
        }

        public bool ChackIfTresueExistsAt(Location a, IPirateGame game)
        {
            game.Debug("Checking if it not working");

            foreach (Treasure t in game.Treasures())
            {
                if (t.Location.Col == a.Col && t.Location.Row == a.Row)
                {
                    game.Debug("Returned True");
                    return true;
                }
            }
            game.Debug("Returned False");
            return false;

        }



        //}


    }

    class ColltionAvoider
    {
        int index = 0;
        public List<Location> SetSailOptions = new List<Location>();
        public bool PlayIn7 = false;

        public Location BestLoction(List<Location> A)
        {
            Location l = A[0];
            foreach (Location loc in A)
            {
                if (!WillCollied(loc))
                {
                    l = loc;
                    SetSailOptions.Add(l);
                    index++;

                    if (index == 3 && PlayIn7)
                    {
                        index = 0;
                        SetSailOptions.Clear();
                    }
                    if (index == 4 && !PlayIn7)
                    {
                        index = 0;
                        SetSailOptions.Clear();
                    }

                    break;
                }
            }
            return l;
        }

        private bool WillCollied(Location l)
        {
            foreach (Location s in SetSailOptions)
            {
                if (s.Col == l.Col && s.Row == l.Row)
                    return true;
            }
            return false;
        }

    }



    //מחלק לקבוצות , בודק מרחק מאוצרות, מראה מרחק הכי קטן


    class TresureDistance
    {
        public List<Treasure> Tresures = new List<Treasure>();
        IPirateGame game { get; set; }
        public TresureDistance(List<Treasure> t, IPirateGame Game)
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


            Treasure T = Tresures[index];
            game.Debug("******************************************************Closest Tresure at " + T.Location.Col + "  : " + T.Location.Row);
            Tresures.RemoveAt(index);

            return T;

        }
    }





    class ShotingDinmaic
    {

    }


    ///////////////////////////////



    public class PirateWithSelectedTresures
    {
        public Pirate pirate { get; set; }
        public Location Tresure { get; set; }
        public PirateWithSelectedTresures(Pirate p, Location t)
        {
            this.pirate = p;
            this.Tresure = t;
        }
        public int Distance(IPirateGame game)
        {
            return game.Distance(pirate.Location, Tresure);
        }
    }

    public class PirateWithSelectedTresureCollction
    {
        public List<PirateWithSelectedTresures> list = new List<PirateWithSelectedTresures>();

        public void Add(PirateWithSelectedTresures pws)
        {
            if (list.Count == 4)
            {
                
                list.Clear();
                list.Add(pws);
            }
            else
            {
                list.Add(pws);
            }
        }



        public List<int> ReturnFarFromTarget(IPirateGame game)
        {
            List<int> answer = new List<int>();
            foreach (var a in list)
            {
                answer.Add(a.Distance(game));
            }
            return answer;
        }


        public PirateWithSelectedTresures this[int key]
        {
            get
            {
                return list[key];
            }
        }
    }

    class MovesOptions
    {


        public IPirateGame game { private get; set; }
        public PirateWithSelectedTresureCollction list = new PirateWithSelectedTresureCollction();


        public int[] RegularGameMoves()
        {
            int Left = 6;
            int Count = 0;
            int[] Answer = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (list[i].pirate.HasTreasure)
                {
                    Answer[i] = 1;
                    Left--;
                    Count++;

                }
            }
            game.Debug("Count ==========================================================> " + Count);
            if (Count == 4)
            {

                return Answer;
            }

            if (Count == 3)
            {
                game.Debug("Inside Count = 3 ************************************************************************");
                for (int i = 0; i < 4; i++)
                {
                    if (!list[i].pirate.HasTreasure)
                    {
                        if (list[i].Distance(game) <= 3)
                            Answer[i] = list[i].Distance(game);
                        else
                        {
                            Answer[i] = 3;
                        }
                        break;
                    }
                }

                return Answer;
            }
            if (Count == 2)
            {
                int Far1 = -1, index1 = -1, index2 = -1, Far2 = -1;

                for (int i = 0; i < 4; i++)
                {

                    if (Answer[i] != 1 && Far1 != -1)
                    {
                        Far2 = list[i].Distance(game);
                        index2 = i;
                    }
                    if (Answer[i] != 1 && Far1 == -1)
                    {
                        Far1 = list[i].Distance(game);
                        index1 = i;
                    }


                }
                if (Math.Min(Far1, Far2) == Far1)
                {
                    if (list[index1].Distance(game) <= 4)
                    {
                        Answer[index1] = list[index1].Distance(game) ;
                        Answer[index2] = 4 - list[index1].Distance(game) ;
                    }
                    else
                    {
                        Answer[index1] = 4;
                        Answer[index2] = 0;
                    }
                }
                else
                {
                    if (list[index2].Distance(game) <= 4)
                    {
                        Answer[index1] = 4 - list[index2].Distance(game) ;
                        Answer[index2] = list[index2].Distance(game) ;
                    }
                    else
                    {
                        Answer[index1] = 0;
                        Answer[index2] = 4;
                    }
                }

                return Answer;
            }

            if (Count == 1)
            {


                game.Debug("&&&&&&&&&&&&&&&&&&&&&&&&&& =>In Count ==1");
                int Far1 = -1, index1 = -1, index2 = -1, Far2 = -1, index3 = -1, Far3 = -1;

                for (int i = 0; i < 4; i++)
                {

                    if (Answer[i] != 1 && Far2 != -1)
                    {
                        Far3 = list[i].Distance(game);
                        index3 = i;
                    }
                    if (Answer[i] != 1 && Far1 != -1 && Far2 == -1)
                    {
                        Far2 = list[i].Distance(game);
                        index2 = i;
                    }
                    if (Answer[i] != 1 && Far1 == -1)
                    {
                        Far1 = list[i].Distance(game);
                        index1 = i;
                    }



                }
                if (Math.Min(Far1, Far2) == Far1)
                {
                    if (Math.Min(Far1, Far3) == Far1)
                    {
                        if (list[index1].Distance(game) <= 5)
                        {
                            Answer[index1] = list[index1].Distance(game);
                            Answer[index2] = 5 - list[index1].Distance(game);
                        }
                        else {

                            Answer[index1] = 5;
                            Answer[index2] = 0;
                        }
                        Answer[index3] = 0;

                        game.Debug("A -- Index 1 = {0} , Index 2 = {1} , Index 3 = {2}", index1, index2, index3);
                    }
                    else
                    {
                        if (list[index3].Distance(game) <= 5)
                        {
                            Answer[index3] = list[index3].Distance(game);
                            Answer[index1] = 5 - list[index3].Distance(game);
                            Answer[index2] = 0;
                        }
                        else
                        {
                            Answer[index1] = 0;
                            Answer[index2] = 0;
                            Answer[index3] = 5;
                        }
                        game.Debug("A -- Index 1 = {0} , Index 2 = {1} , Index 3 = {2}", index1, index2, index3);
                    }

                }
                else
                {

                    if (Math.Min(Far2, Far3) == Far2)
                    {
                        if (list[index2].Distance(game) <= 5)
                        {
                            Answer[index2] = list[index2].Distance(game);
                            Answer[index1] = 5 - list[index2].Distance(game);
                            Answer[index3] = 0;
                        }
                        else
                        {

                            Answer[index1] = 0;
                            Answer[index2] = 5;
                            Answer[index3] = 0;
                        }
                        game.Debug("A -- Index 1 = {0} , Index 2 = {1} , Index 3 = {2}", index1, index2, index3);
                    }
                    else
                    {
                        if (list[index3].Distance(game) <= 5)
                        {
                            Answer[index3] = list[index3].Distance(game);
                            Answer[index2] = 5 - list[index3].Distance(game);
                            Answer[index1] = 0;
                        }
                        Answer[index1] = 0;
                        Answer[index2] = 0;
                        Answer[index3] = 5;
                        game.Debug("A -- Index 1 = {0} , Index 2 = {1} , Index 3 = {2}", index1, index2, index3);
                    }
                }


                return Answer;
            }
            else
            {


                int Min = list[0].Distance(game);
                int Index = 0;

                for (int i = 1; i < 4; i++)
                {
                    if (list[i].Distance(game) < Min)
                    {
                        Min = list[i].Distance(game);
                        Index = i;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (i == Index)
                    {
                        if (list[i].Distance(game) <= 5)
                            Answer[i] = list[i].Distance(game);
                        else
                            Answer[i] = 6;
                    }
                    else
                    {
                        Answer[i] = 0;
                    }
                }



                return Answer;



            }


        }





    }

}
