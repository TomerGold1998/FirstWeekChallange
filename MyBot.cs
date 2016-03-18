using Pirates;
using System;
using System.Collections.Generic;
using DataForLife;


namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        const string a = @"run.bat bots\csharp\ExampleBot\MyBot.cs bots\demoBot1.py";

        List<Treasure> Gotolocations = new List<Treasure>();
        ColltionAvoider CA = new ColltionAvoider();
        bool[] HasLocation = new bool[] { false, false, false, false };
        int index = 0;
       
        public void DoTurn(IPirateGame game)
        {
            if (index == 0)
            {
                DataSending ds = new DataSending();
                ds.SendData(null);
                index++;
            }

            List<PirateWithLocation> PWL = new List<PirateWithLocation>();
            MovingOption mo = new MovingOption(PWL, game);

            List<Pirate> MyPirates = game.MyPirates();
            //  game.Debug("MyPirate[0] == Pirate id {0}", MyPirates[0].Id);
            Location[] Goto = new Location[4];

            for (int i = 0; i < 4; i++)
            {

                //     TryShoot(MyPirates[i], game.EnemyPirates(),game);

                if (!HasLocation[i])
                {

                    Goto[i] = GotoLocation(MyPirates[i], game);
                    PWL.Add(new PirateWithLocation(MyPirates[i], Goto[i], game));

                    game.Debug("Pirate {0} Will Go to Location {1}:{2}", MyPirates[i].Id, Goto[i].Row, Goto[i].Col);
                }
                else
                {
                    if (MyPirates[i].HasTreasure)
                    {
                        Goto[i] = MyPirates[i].InitialLocation;
                        PWL.Add(new PirateWithLocation(MyPirates[i], Goto[i], game));
                        HasLocation[i] = false;
                    }
                    else
                    {

                        Goto[i] = Gotolocations[Gotolocations.Count - 4 + MyPirates[i].Id].Location;
                        if (!GameHasTresure(Goto[i], game))
                            Goto[i] = Goto[i] = GotoLocation(MyPirates[i], game);

                        PWL.Add(new PirateWithLocation(MyPirates[i], Goto[i], game));
                        game.Debug("Pirate {0} Will Go to Location {1}:{2}", MyPirates[i].Id, Goto[i].Row, Goto[i].Col);
                    }
                }
            }
            mo.list = PWL;
            int[] Answer = mo.DoRegularTurn();
            game.Debug("Answer == {0} , {1} , {2}, {3}", Answer[0], Answer[1], Answer[2], Answer[3]);
            for (int i = 0; i < 4; i++)//To Add- Check Location is closest to selected player;;;;
            {

                List<Location> Options = game.GetSailOptions(MyPirates[i], Goto[i], Answer[i]);
                Location locationToGo = CA.BestLoction(Options, MyPirates[i], game);
                if (!ComperLocations(locationToGo, MyPirates[MyPirates[i].Id].Location))
                {
                    //    game.Debug("Pirate {0} Is Going to {1}-{2]", MyPirates[i].Id, locationToGo.Row, locationToGo.Col);
                    game.SetSail(MyPirates[MyPirates[i].Id], locationToGo);
                }

            }

        }

        private bool GameHasTresure(Location location, IPirateGame game)
        {

            List<Treasure> AllLocations = game.Treasures();
            foreach (Treasure t in AllLocations)
            {
                if (ComperLocations(t.Location, location))
                {
                    return true;
                }
            }
            return false;
        }

        private void TryShoot(Pirate pirate, List<Pirate> list, IPirateGame game)
        {
            foreach (Pirate p in list)
                if (game.Distance(pirate, p) <= game.GetAttackRadius())
                {
                    game.Attack(pirate, p);
                }
        }

        public bool ComperLocations(Location a, Location b)
        {
            return (a.Col == b.Col && a.Row == b.Row);
        }
        public Location GotoLocation(Pirate p, IPirateGame Game)
        {
            if (p.HasTreasure)
            {
                HasLocation[p.Id] = false;
                return p.InitialLocation;
            }

            List<Treasure> AllLocations = Game.Treasures();

            int[] Distances = new int[Game.Treasures().Count];

            for (int i = 0; i < Distances.Length; i++)
                Distances[i] = Game.Distance(p, AllLocations[i].Location);

            for (int i = 0; i < Distances.Length - 1; i++)
            {
                for (int j = i + 1; j < Distances.Length; j++)
                {
                    if (Distances[i] > Distances[j])
                    {
                        int Temp = Distances[i];
                        Distances[i] = Distances[j];
                        Distances[j] = Temp;

                        Treasure TresureTemp = AllLocations[i];
                        AllLocations[i] = AllLocations[j];
                        AllLocations[j] = TresureTemp;
                    }
                }
            }


            for (int i = 0; i < AllLocations.Count; i++)
            {
                if (!Gotolocations.Contains(AllLocations[i]))
                {
                    Gotolocations.Add(AllLocations[i]);
                    HasLocation[p.Id] = true;
                    return AllLocations[i].Location;
                }
            }

            return p.InitialLocation;




        } //Add - Not More than one pirate over a location
    }

    class ColltionAvoider
    {
        int index = 0;
        public List<Location> SetSailOptions = new List<Location>();


        public Location BestLoction(List<Location> A, Pirate p, IPirateGame game)
        {
            Location l = p.Location;
            foreach (Location loc in A)
            {
                if (!WillCollied(loc))
                {
                    l = loc;

                    break;
                }
            }

            if (index == 4)
            {
                this.SetSailOptions.Clear();
                index = 0;
                game.Debug("Set Sail  Reseted");
            }
            index++;
            SetSailOptions.Add(l);

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

    class PirateWithLocation
    {
        IPirateGame game;
        public Pirate p;
        public Location l;
        public PirateWithLocation(Pirate pirate, Location loc, IPirateGame game)
        {
            this.p = pirate;
            this.l = loc;
            this.game = game;
        }
        public int Distance()
        {
            return game.Distance(p.Location, l);
        }
    }
    class MovingOption
    {
        IPirateGame game;
        public List<PirateWithLocation> list = new List<PirateWithLocation>();
        public MovingOption(List<PirateWithLocation> List, IPirateGame game)
        {
            this.list = List;
            this.game = game;
        }

        public int[] DoRegularTurn()
        {
            int Left = 6;
            int Count = 0;
            int[] Answer = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (list[i].p.HasTreasure)
                {
                    Answer[i] = 1;
                    Left--;
                    Count++;
                }
            }
            if (Count == 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (list[i].p.HasTreasure)
                    {
                        Answer[i] = 1;

                    }
                    else
                    {
                        if (list[i].Distance() < 3)
                        {
                            Answer[i] = list[i].Distance();
                        }
                        else
                        {
                            Answer[i] = 3;
                        }
                    }
                }
                return Answer;
            }
            game.Debug("Count == {0}", Count);

            if (Count == 4)
            {
                return Answer;
            }
            else
            {
                return FindBest(list, 6 - Count);
            }

        }

        private int[] FindBest(List<PirateWithLocation> list, int StepLeft)
        {
            game.Debug("Step Left == {0}", StepLeft);
            int[] Answer = new int[] { 0, 0, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                if (!list[i].p.HasTreasure)
                {
                    game.Debug("Pirate {0} Donest have a tresure", i);


                    if (list[i].Distance() == StepLeft)
                    {
                        Answer[i] = StepLeft;
                        game.Debug("Used all Moves On Pirate {0}", i);
                        return Answer;

                    }
                    else
                    {
                        if (list[i].Distance() < StepLeft)
                        {

                            for (int j = 0; j < 4; j++)
                            {
                                if (i != j && !list[j].p.HasTreasure)
                                {
                                    Answer[j] = StepLeft - list[i].Distance();
                                    game.Debug("Gave {0} {1} Moves", j, Answer[j]);
                                    break;
                                }


                            }
                            for (int j = 0; j < 4; j++)
                            {
                                if (list[j].p.HasTreasure)
                                {
                                    Answer[j] = 1;
                                }
                            }


                            Answer[i] = list[i].Distance();
                            game.Debug("Gave {0} {1} Moves", i, Answer[i]);
                            return Answer;

                        }
                        else
                        {
                            if (i == 3 && StepLeft == 6)
                            {
                                game.Debug("6 LEFT - ALL GIVED EQULALY");
                                Answer[0] = 1;
                                Answer[1] = 2;
                                Answer[2] = 1;
                                Answer[3] = 2;
                                return Answer;
                            }
                            else
                            {
                                if (i == 3)
                                {
                                    return FreeGo(list, StepLeft);
                                }

                            }

                        }
                    }

                }
                else
                {
                    Answer[i] = 1;
                    if (i == 3)
                    {
                        if ((Answer[0] + Answer[1] + Answer[2] + Answer[3]) < 6)
                        {
                            return FreeGo(list, StepLeft);
                        }
                    }
                }

            }
            return Answer;
        }

        private int[] FreeGo(List<PirateWithLocation> list, int StepLeft)
        {
            game.Debug("************************************IN FreeGo");
            int[] Answer = new int[] { 0, 0, 0, 0 };
            //
            if (StepLeft == 5)
            {
                game.Debug("Number of steps = 5");
                bool FoundOne = false;
                for (int i = 0; i < 4; i++)
                {
                    if (list[i].p.HasTreasure)
                    {
                        Answer[i] = 1;
                        game.Debug("GAVE Pirate {0} - {1} Move", i, 1);

                    }
                    else
                    {
                        if (!FoundOne)
                        {
                            Answer[i] = 1;
                            FoundOne = true;
                            game.Debug("GAVE Pirate {0} - {1} Move", i, 1);
                        }
                        else
                        {
                            Answer[i] = 2;
                            game.Debug("GAVE Pirate {0} - {1} Move", i, 2);
                        }
                    }
                }
                return Answer;
            }
            // 
            if (StepLeft == 4)
            {
                game.Debug("Number of steps = 4");
                for (int i = 0; i < 4; i++)
                {
                    if (list[i].p.HasTreasure)
                    {
                        Answer[i] = 1;
                        game.Debug("GAVE Pirate {0} - {1} Move", i, 1);

                    }
                    else
                    {
                        Answer[i] = 2;
                        game.Debug("GAVE Pirate {0} - {1} Move", i, 2);
                    }
                }
                return Answer;
            }
            else
            {
                game.Debug("Number of steps = {0}", StepLeft);
                for (int i = 0; i < 4; i++)
                {
                    if (list[i].p.HasTreasure)
                    {
                        Answer[i] = 1;
                        game.Debug("GAVE Pirate {0} - {1} Move", i, 1);

                    }
                    else
                    {
                        Answer[i] = 3;
                        game.Debug("GAVE Pirate {0} - {1} Move", i, 3);

                    }
                }
                return Answer;
            }
        }
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
}


