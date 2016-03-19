using Pirates;
using System;
using System.Collections.Generic;



namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        const string a = @"run.bat bots\csharp\ExampleBot\MyBot.cs bots\demoBot1.py";

        List<Treasure> Gotolocations = new List<Treasure>();
        ColltionAvoider CA = new ColltionAvoider();
        bool[] HasLocation = new bool[] { false, false, false, false };

        OneONOneOption One = new OneONOneOption();
        Collector C = new Collector();
        AttackingSpree AS = new AttackingSpree();
        int index = 0;
        public void DoTurn(IPirateGame game)
        {
            List<Pirate> MyPirates = game.MyPirates();
            //if (index == 0)
            //{
            //    DataForLife.DataSending ds = new DataSending();
            //    ds.SendData(null);
            //    index++;
            // }

            /*   List<PirateWithLocation> PWL = new List<PirateWithLocation>();
               MovingOption mo = new MovingOption(PWL, game);

               List<Pirate> MyPirates = game.MyPirates();
   */
            if (MyPirates.Count == 1)
            {
                if (index == 0)
                {
                    One.Tresure = game.Treasures()[0];
                    index++;
                }
                One.EnemyPirate = game.EnemyPirates()[0];
                One.MyPlayer = MyPirates[0];
                One.Game = game;
                One.OneONOneTurn();
            }
            else
            {
                AS.EnemyPirates = game.EnemyPirates();
                AS.MyPirates = game.MyPirates();
                AS.Game = game;
                AS.DoTurn();
            }
            /*
        else
        {

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
      */
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
    // For One On Ones 
    public class OneONOneOption
    {

        public Treasure Tresure;
        public Pirate MyPlayer;
        public Pirate EnemyPirate;
        public IPirateGame Game;

        bool HasShoot = false;
        bool IsEnemyDown = false;

        public void OneONOneTurn()
        {
            if (!MyPlayer.HasTreasure && !HasShoot && !IsEnemyDown)
            {

                if (Game.InRange(MyPlayer, EnemyPirate))
                {
                    Game.Debug("Pirate Is In Range");
                    Game.Attack(MyPlayer, EnemyPirate);
                    Game.Debug("Attacked Enmey Pirate");
                    HasShoot = true;
                    IsEnemyDown = true;
                    return;
                }
                else
                {
                    Game.Debug("Going to Attack Enmey Pirate");
                    Location Destnation = new Location(Tresure.Location.Row + 1, Tresure.Location.Col + 1);
                    Game.SetSail(MyPlayer, Game.GetSailOptions(MyPlayer, Destnation, 6)[0]);
                }
            }
            else
            {
                if (HasShoot && !MyPlayer.HasTreasure)
                {
                    Game.Debug("Go To Treuseure");
                    Location Destnation = new Location(Tresure.Location.Row, Tresure.Location.Row);
                    Game.SetSail(MyPlayer, Game.GetSailOptions(MyPlayer, Destnation, 1)[0]);
                }
                if (MyPlayer.HasTreasure)
                {
                    Game.Debug("Go To Home");
                    if (MyPlayer.Location.Col != MyPlayer.InitialLocation.Col)
                        Game.SetSail(MyPlayer, Game.GetSailOptions(MyPlayer, new Location(MyPlayer.Location.Row, MyPlayer.InitialLocation.Col), 1)[0]);
                    else
                    {
                        Game.SetSail(MyPlayer, Game.GetSailOptions(MyPlayer, MyPlayer.InitialLocation, 1)[0]);
                    }
                }
            }
        }
    } // Done

    public class CollitionCoruse
    {
        public IPirateGame game;
        public List<Pirate> MyPirate = new List<Pirate>();
        public List<Pirate> Enemy = new List<Pirate>();


        public void Turn()
        {
            List<Pirate> Targets = new List<Pirate>();
            foreach (Pirate e in Enemy)
            {
                if (e.HasTreasure)
                    Targets.Add(e);
            }
            if (Targets != null)
            {
                Targets = SortTarget(Targets, MyPirate[3]);
                if (game.InRange(MyPirate[3], Targets[0]) && MyPirate[3].ReloadTurns == 0 && !MyPirate[3].HasTreasure)
                {
                    game.Debug("Pirate 3 is attacing pirate number {0}", Targets[0].Id);
                    game.Attack(MyPirate[3], Targets[0]);
                }
                else
                {
                    if (MyPirate[3].ReloadTurns != 0)
                    {

                    }
                }

            }


        }

        private List<Pirate> SortTarget(List<Pirate> Targets, Pirate p)
        {
            int[] Distances = new int[Targets.Count];

            for (int i = 0; i < Distances.Length; i++)
                Distances[i] = game.Distance(p, Targets[i]);

            for (int i = 0; i < Distances.Length - 1; i++)
            {
                for (int j = i + 1; j < Distances.Length; j++)
                {
                    if (Distances[i] > Distances[j])
                    {
                        int Temp = Distances[i];
                        Distances[i] = Distances[j];
                        Distances[j] = Temp;

                        Pirate Pi = Targets[i];
                        Targets[i] = Targets[j];
                        Targets[j] = Pi;
                    }
                }
            }
            return Targets;
        }


    }

    public class Collector
    {
        /// <summary>
        /// GotoMiddleuntil One Has Trsure 
        /// IF Enemy Has tresure shoot him
        /// go pick up a trusre 
        /// second go with five moves 
        /// ...
        /// </summary>

        public int Moves = 6;
        public IPirateGame game;
        public List<Pirate> MyPirate = new List<Pirate>();
        public List<Pirate> Enemy = new List<Pirate>();
        public List<Treasure> Tresures = new List<Treasure>();

        private bool Hit = false;
        private Pirate Killer = null;
        private Treasure KillerTresure = null;
        private Pirate LastKiller = null;
        public void DoTurn()
        {
            foreach (Pirate p in MyPirate)
            {
                if (p.HasTreasure)
                {
                    Moves--;
                }
            }

            List<Pirate> Targets = new List<Pirate>();
            foreach (Pirate e in Enemy)
            {
                if (e.HasTreasure)
                {
                    game.Debug("Enemy Pirate number {0} Has Tresure");
                    Targets.Add(e);
                }
            }
            // Check if any Enemy has Treesure
            if (Killer == null)
            {
                game.Debug("Game Dosent have a killer");
                if (Targets.Count != 0)
                {
                    game.Debug("Have Targets");
                    // Send Pirate With Moves To Kill
                    Killer = CheckClosestPirate();

                    if (Killer != null)
                    {
                        //Check if Killer at Targetb
                        if (game.InRange(Killer, Targets[0]))
                        {
                            game.Debug("Killer attacking enemy pirate {0} ", Targets[0].Id);
                            game.Attack(Killer, Targets[0]);
                            // Update Killer Target
                            KillerTresure = FindColsestTresure(Killer, game);
                            game.Debug("Killer Trsure is {0}", KillerTresure.Id);
                            LastKiller = Killer;
                            Killer = null;
                            return;
                        }
                        else
                        {
                            game.Debug("Killer is going to target {0} - {1} ", Targets[0].Location.Row, Targets[0].Location.Col);
                            // Send Killer with all Moves to Kill Target
                            Location A = game.GetSailOptions(Killer, new Location(Targets[0].InitialLocation.Row, Targets[0].InitialLocation.Col + 1), Moves)[0];
                            game.Debug("Killer going to be at {0} - {1} ", A.Row, A.Col);
                            game.SetSail(Killer, A);
                        }
                    }
                }
                else
                {
                    Killer = CheckClosestPirate();
                }
            }
            else
            {
                game.Debug("Game has Killer ");
                if (Targets.Count != 0)
                {

                    game.Debug("Killer is going to target {0} - {1} ", Targets[0].InitialLocation.Row, Targets[0].InitialLocation.Col - 1);
                    //Send killer to target
                    Location A = game.GetSailOptions(Killer, new Location(Targets[0].InitialLocation.Row, Targets[0].InitialLocation.Col - 1), Moves)[0];
                    game.Debug("Killer going to be at {0} - {1} ", A.Row, A.Col);
                    game.SetSail(Killer, A);


                }
                else
                {
                    game.Debug("Killer is going to somewhere");
                    //send Killer to Enemy Location 
                    Location A = game.GetSailOptions(Killer, new Location(11, 16), Moves)[0];
                    game.Debug("Killer going to be at {0} - {1} ", A.Row, A.Col);
                    game.SetSail(Killer, A);
                }
            }
            if (LastKiller != null)
            {
                if (!LastKiller.HasTreasure)
                {
                    Location A = game.GetSailOptions(LastKiller, KillerTresure, Moves)[0];
                    game.Debug("Killer going to be at {0} - {1} ", A.Row, A.Col);
                    game.SetSail(LastKiller, A);
                }
                else
                {
                    LastKiller = null;
                    KillerTresure = null;
                }
            }

            foreach (Pirate p in MyPirate)
            {
                if (p.HasTreasure)
                {
                    game.SetSail(p, game.GetSailOptions(p, p.InitialLocation, 1)[0]);
                }
            }


            Moves = 6;

        }

        private Treasure FindColsestTresure(Pirate Killer, IPirateGame game)
        {
            int[] Distance = new int[game.Treasures().Count];
            List<Treasure> tresuress = game.Treasures();
            for (int i = 0; i < Distance.Length; i++)
            {
                Distance[i] = game.Distance(Killer.Location, tresuress[i].Location);
            }

            for (int i = 0; i < Distance.Length - 1; i++)
            {
                for (int j = i + 1; j < Distance.Length; j++)
                {
                    if (Distance[i] > Distance[j])
                    {
                        int Temp = Distance[i];
                        Distance[i] = Distance[j];
                        Distance[j] = Temp;

                        Treasure TresureTemp = tresuress[i];
                        tresuress[i] = tresuress[j];
                        tresuress[j] = TresureTemp;
                    }
                }
            }
            return tresuress[0];


        }

        private Pirate CheckClosestPirate()
        {

            foreach (Pirate p in MyPirate)
            {
                if (!p.HasTreasure && p.ReloadTurns == 0)
                {
                    game.Debug("Pirate {0} is the Killer", p.Id);
                    return p;
                }
            }
            return null;
        }

    }





    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    class AttackingSpree
    {
        public List<Pirate> MyPirates = new List<Pirate>();
        public List<Pirate> EnemyPirates = new List<Pirate>();
        public IPirateGame Game { get; set; }



        public void DoTurn()
        {

            List<Pirate> Targets = new List<Pirate>();
            List<Pirate> PossibaleAttackers = new List<Pirate>();

            //Check whitch pirates are targets
            foreach (Pirate p in EnemyPirates)
            {
                if (p.HasTreasure)
                {
                    Targets.Add(p);
                    Game.Debug("")
                }
            }


            foreach (Pirate p in MyPirates)
            {
                if (!p.HasTreasure && p.ReloadTurns == 0)
                {
                    PossibaleAttackers.Add(p);
                }
            }

            if (Targets.Count == 0 || PossibaleAttackers.Count == 0)
            {
                RegularTurn();
            }
            else
            {
                Pirate[] Answer = FindBestPirate(PossibaleAttackers, Targets);

                //Posible error
                bool Possible = CheckIfPossible(Answer[0], Answer[1]);
                if (Possible)
                {
                    if (Game.InRange(Answer[0].Location, Answer[1].Location))
                    {
                        Game.Debug("Pirate Number {0} Has Attacked Enemy Pirate Number {1}", Answer[0].Id, Answer[1].Id);
                        Game.Attack(Answer[0], Answer[1]);
                        return;
                    }
                    else
                    {
                        if (Answer[0].Location.Row == Answer[1].InitialLocation.Row && Answer[0].Location.Col == Answer[1].Location.Col - 1)
                        {
                            Location Destination = Game.GetSailOptions(Answer[0], Answer[1].Location, 6)[0];
                            Game.SetSail(Answer[0], Destination);
                        }
                        else
                        {
                            Location Destination = Game.GetSailOptions(Answer[0], new Location(Answer[1].InitialLocation.Row, Answer[1].InitialLocation.Col - 1), 6)[0];
                        }
                    }
                }
                else
                {
                    RegularTurn();
                }


            }
        }

        private void RegularTurn()
        {
            int Moves = 6;
            foreach (Pirate p in MyPirates)
            {
                if (p.HasTreasure)
                {
                    Location Destination = Game.GetSailOptions(p, p.InitialLocation, 1)[0];
                    Game.SetSail(p, Destination);
                    Moves--;
                }
            }

            TresureOptions TO = new TresureOptions();
            TO.game = Game;
            TO.MyPirate = Game.MyPiratesWithoutTreasures();
            PirateWithTresure PWT = TO.BestPirateWithTresue();
            Location l = Game.GetSailOptions(PWT.pirate, PWT.Tresure, Moves)[0];
            Game.SetSail(PWT.pirate, l);



            //Find NearestTrsure ;
            // Find Super Near ;
            // Send With six;
        }

        public bool CheckIfPossible(Pirate MY, Pirate Enemy)
        {
            return Game.Distance(Enemy.Location, Enemy.InitialLocation) > (Game.Distance(MY.Location, Enemy.InitialLocation) / 6);
        }


        private Pirate[] FindBestPirate(List<Pirate> PossibaleAttackers, List<Pirate> Targets)
        {
            Pirate[] Answers = new Pirate[2];
            int[,] Distancese = new int[PossibaleAttackers.Count, Targets.Count];
            for (int i = 0; i < Distancese.GetLength(0); i++)
            {
                for (int j = 0; j < Distancese.GetLength(1); j++)
                {
                    Distancese[i, j] = Game.Distance(PossibaleAttackers[i].Location, Targets[j].Location);
                }
            }
            int Min = Distancese[0, 0];
            int[] index = new int[] { 0, 0 };

            for (int i = 0; i < Distancese.GetLength(0); i++)
            {
                for (int j = 0; j < Distancese.GetLength(1); j++)
                {
                    if (Min > Game.Distance(PossibaleAttackers[i].Location, Targets[j].Location))
                    {
                        Min = Game.Distance(PossibaleAttackers[i].Location, Targets[j].Location);
                        index[0] = i;
                        index[1] = j;
                    }
                }
            }
            Answers[0] = PossibaleAttackers[index[0]];
            Answers[1] = Targets[index[1]];
            return Answers;


        }
    }


    class TresureOptions
    {
        public List<Pirate> MyPirate;
        public IPirateGame game;

        public PirateWithTresure BestPirateWithTresue()
        {
            List<Treasure> AllTreessures = game.Treasures();


            int[,] Distancese = new int[MyPirate.Count, AllTreessures.Count];
            for (int i = 0; i < Distancese.GetLength(0); i++)
            {
                for (int j = 0; j < Distancese.GetLength(1); j++)
                {
                    Distancese[i, j] = game.Distance(MyPirate[i].Location, AllTreessures[j].Location);
                }
            }
            int Min = Distancese[0, 0];
            int[] index = new int[] { 0, 0 };

            for (int i = 0; i < Distancese.GetLength(0); i++)
            {
                for (int j = 0; j < Distancese.GetLength(1); j++)
                {
                    if (Min > game.Distance(MyPirate[i].Location, AllTreessures[j].Location))
                    {
                        Min = game.Distance(MyPirate[i].Location, AllTreessures[j].Location);
                        index[0] = i;
                        index[1] = j;
                    }
                }
            }
            PirateWithTresure Answer = new PirateWithTresure(MyPirate[index[0]], AllTreessures[index[1]], game);
            return Answer;
        }
    }
    class PirateWithTresure
    {
        public Pirate pirate;
        public Treasure Tresure;
        public IPirateGame game;
        public PirateWithTresure(Pirate p, Treasure t, IPirateGame Game)
        {
            this.pirate = p;
            this.Tresure = t;
            this.game = Game;
        }
        public int Distance()
        {
            return game.Distance(pirate.Location, Tresure.Location);
        }

    }
}



