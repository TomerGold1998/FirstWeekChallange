using System.Collections.Generic;
using Pirates;

namespace MyBot
{
    #region game
    public class MyBot : Pirates.IPirateBot
    {
        GameLogic GL = new GameLogic();
        public void DoTurn(IPirateGame game)
        {
            GL.BasicStratgey(game);
        }
    }
    #endregion

    #region Utils

    class ColltionAvoider
    {
        private List<Location> CaptureLocation = new List<Location>();

        public IPirateGame Game;

        public void Reset()
        {
            this.CaptureLocation.Clear();
        }
        public void Init(bool withTresuars)
        {
            if (Game.MyDrunkPirates() != null)
                foreach (Pirate p in Game.MyDrunkPirates()) // ספינות שלי שיכורות
                {
                    CaptureLocation.Add(p.Location);
                }

            if (Game.EnemyDrunkPirates() != null)
                foreach (Pirate p in Game.EnemyDrunkPirates()) // ספינות אוייב שיכורות
                {
                    CaptureLocation.Add(p.Location);
                }

            if (Game.EnemyPiratesWithTreasures() != null)
                foreach (Pirate p in Game.EnemyPiratesWithTreasures()) // ספינות אוייב עם אוצר
                {
                    List<Location> ETW = Game.GetSailOptions(p, p.InitialLocation, 1);
                    foreach (Location l in ETW)
                    {
                        CaptureLocation.Add(l);
                    }
                }

            if (Game.MyPirates() != null)
                foreach (Pirate p in Game.MyPirates()) // ספינות שלי 
                {
                    CaptureLocation.Add(p.Location);
                }

            if (withTresuars)
            {
                foreach (Treasure T in Game.Treasures())
                {
                    CaptureLocation.Add(T.Location);
                }
            }
        }


        public Location TryAdd(List<Location> PossibleLocations)
        {

            foreach (Location l in PossibleLocations)
            {
                if (!CaptureLocation.Contains(l))
                {
                    CaptureLocation.Add(l);
                    Game.Debug("Location Added {0}-{1}", l.Row, l.Col);
                    return l;
                }
            }
            Game.Debug("Did not Found Soultion");
            return null;
        }
    }
    #endregion

    #region Tactics

    class HomeSick
    {
        public Pirate pirate;
        public IPirateGame game;
        public ColltionAvoider CA;

        public void DoHomeTurn()
        {
            game.Debug("Pirate {0} is in home mode", pirate.Id);
            Location Dest = CA.TryAdd(game.GetSailOptions(pirate, pirate.InitialLocation, pirate.CarryTreasureSpeed));
            game.Debug("Moves ={0}", pirate.CarryTreasureSpeed);
            if (ShouldDefence())
            {
                if (canDefence())
                {
                    game.Debug("Defending...");
                    game.Defend(pirate);
                }
                else
                {
                    game.Debug("Cant turn on defence, try to move away");
                    game.SetSail(pirate, Dest);
                }
            }
            else
            {
                game.Debug("Moving towrds home");
                game.SetSail(pirate, Dest);
            }


        }

        private bool canDefence()
        {
            return pirate.DefenseExpirationTurns == 0 && pirate.DefenseReloadTurns == 0;
        }

        private bool ShouldDefence()
        {
            List<Pirate> PossibleAttackers = game.EnemyPiratesWithoutTreasures();
            if (PossibleAttackers.Count > 0)
            {
                foreach (Pirate p in PossibleAttackers)
                {
                    if (game.InRange(p.Location, pirate.Location))
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }
    class Attacking
    {
        public Pirate Attacker;
        public Pirate Target;
        public IPirateGame game;
        public ColltionAvoider CA;
        public int MovesLeft = 0;
        public bool HasShot = false;
        public bool hasmoves = false;
        public bool Chase = false;
        public void AttackerTurn()
        {
            game.Debug("Pirate {0} is in attacking Mode", Attacker.Id);

            if (game.InRange(Attacker.Location, Target.Location) && Target.DefenseExpirationTurns == 0)
            {
                game.Debug("Pirate {0} Shotting at pirate {1}", Attacker.Id, Target.Id);
                game.Attack(Attacker, Target);
                HasShot = true;
                hasmoves = true;
                return;
            }
            else
            {
                if (!game.InRange(Attacker.Location, Target.Location))
                {
                    game.Debug("Check If Can Reach Destination");
                    if (CanReach())
                    {
                        game.Debug("Goto destination");
                        Location dest = CA.TryAdd(game.GetSailOptions(Attacker, Target.Location, game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count));
                        if (dest != null)
                        {
                            MovesLeft = game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count;
                            game.Debug("Moves = {0}", game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count);
                            game.Debug("Location = {0}-{1}, Target at ={2}-{3}", dest.Row, dest.Col, Target.Location.Row, Target.Location.Col);
                            game.SetSail(Attacker, dest);
                            hasmoves = true;
                        }
                    }
                    else
                    {
                        hasmoves = false;
                        game.Debug("Cannot reach target ");
                    }

                }
                else
                {
                    this.Chase = true;
                    Chaser c = new Chaser();
                    c.CA = CA;
                    c.game = game;
                    c.Chser = Attacker;
                    c.Target = Target;
                    c.ChaserTurn();
                    //chase target;
                    this.MovesLeft = c.Moves;
                }
            }

        }

        private bool CanReach()
        {
            game.Debug("Rigth side == {0} ", (game.Distance(Target.Location, Target.InitialLocation) / (Target.CarryTreasureSpeed)));
            game.Debug("Left Side == {0}", (game.Distance(Attacker.Location, Target.Location) / (game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count)));
            return ((game.Distance(Target.Location, Target.InitialLocation) / (Target.CarryTreasureSpeed)) > (game.Distance(Attacker.Location, Target.Location) / (game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count)));
        }


        public void FindBestMatch(List<Pirate> MyPirates, List<Pirate> EnemyPirates)
        {
            int Min = 100;
            int PirateIndex = 0;
            int EnemyIndex = 0;
            int PirateCount = 0;
            int EnemyCount = 0;

            foreach (Pirate p in MyPirates)
            {
                EnemyCount = 0;
                foreach (Pirate e in EnemyPirates)
                {
                    if (game.Distance(p.Location, e.Location) < Min)
                    {
                        Min = game.Distance(p.Location, e.Location);
                        EnemyIndex = EnemyCount;
                        PirateIndex = PirateCount;

                    }
                    EnemyCount++;
                }
                PirateCount++;
            }

            game.Debug("Min Distance from target = {0} ");
            this.Target = EnemyPirates[EnemyIndex];
            this.Attacker = MyPirates[PirateIndex];
            game.Debug("Attacker = {0} , Target = {1}", Attacker.Id, Target.Id);

        }

    }

    class Chaser
    {
        public Pirate Chser;
        public Pirate Target;
        public IPirateGame game;
        public ColltionAvoider CA;
        public int Moves = 0;

        public void ChaserTurn()
        {
            game.Debug("Pirate {0} is a chaser", Chser.Id);
            Location Dest = game.GetSailOptions(Target, Target.InitialLocation, Target.CarryTreasureSpeed)[0];
            Location Goto = CA.TryAdd(game.GetSailOptions(Chser, Dest, Target.CarryTreasureSpeed));
            Moves = Target.CarryTreasureSpeed;
            game.Debug("Going To Location {0} -{1}", Goto.Row, Goto.Col);
            game.SetSail(Chser, Goto);
        }
    }


    class Crashing
    {

        public IPirateGame game;
        public Pirate Player;
        public Pirate Enemy;
        public int Moves = 0;
        public bool HasCompleted = true;



        public void DoTurn()
        {
            game.Debug("Pirate = {0}, Mode  = Crashing ",Player.Id);
            Enemy = FindBestEnemy();
            if (Enemy != null)
            {
                if (game.Distance(Enemy.Location, Player.Location) < game.GetActionsPerTurn())
                {
                    Player = ReFindPlayer();
                    Enemy = ReFindEnemy();
                    Moves = game.Distance(Enemy.Location, Player.Location) + 1;
                    Location Dest1 = game.GetSailOptions(Enemy, Enemy.InitialLocation, Enemy.CarryTreasureSpeed)[0];
                    Location Dest = game.GetSailOptions(Player, Dest1, Moves)[0];
                    game.Debug("Moving to Location {0}-{1}", Dest.Row, Dest.Col);
                   
                    game.SetSail(Player, Dest);
                    HasCompleted = true;
                }
                else
                {
                    Moves = game.GetActionsPerTurn();
                    Location Dest1 = game.GetSailOptions(Enemy, Enemy.InitialLocation, Enemy.CarryTreasureSpeed)[0];
                    Location Dest = game.GetSailOptions(Player, Dest1, Moves)[0];
                    game.Debug("Moving to Location {0}-{1}", Dest.Row, Dest.Col);
                    game.SetSail(Player, Dest);
                    HasCompleted = true;
                }
            }
            else
            {
                game.Debug("Did not found Enemy, Change tactics ");
                HasCompleted = false;
            }
        }

        private Pirate ReFindEnemy()
        {
            foreach (Pirate p in game.EnemyPiratesWithTreasures())
            {
                if (p.Id == Player.Id)
                    return p;
            }
            return null;
        }

        private Pirate ReFindPlayer()
        {
            foreach (Pirate p in game.MyPirates())
            {
                if (p.Id == Player.Id)
                    return p;
            }
            return null;
        }
        private Pirate FindBestEnemy()
        {

            List<Pirate> EnemyWithTresure = game.EnemyPiratesWithTreasures();
            int min = 1000;
            int index = 0;
            int count = 0;
            if (EnemyWithTresure.Count == 0)
            {
                return null;
            }
            else
            {
                foreach (Pirate p in EnemyWithTresure)
                {
                    if (game.Distance(p.Location, Player.InitialLocation) < min)
                    {
                        min = game.Distance(p.Location, Player.InitialLocation);
                        index = count;
                    }
                    count++;
                }
                return EnemyWithTresure[index];
            }
        }
    }

    class Collectors
    {
        Pirate Collector;
        Treasure Target;
        public IPirateGame Game;
        public ColltionAvoider CA;

        public int Moves = 0;

        public void DoTurn()
        {
            Game.Debug("Pirate {0} in Collector", Collector.Id);
            Game.Debug("C 1");
            Location Dest  = CA.TryAdd(Game.GetSailOptions(Collector,Target.Location,Moves));
            Game.Debug("C 2");
            Game.SetSail(Collector,Dest);
            Game.Debug("C 3");
        }

        public void Findbest(List<Pirate> Pirates, IPirateGame game)
        {
            int MinTresure = 10000;
            int TresureIndex = 0;
            int TresureCount = 0;
            int PiratesIndex = 0;
            int PiratesCount = 0;

            foreach (Pirate p in Pirates)
            {
                TresureCount = 0;
                foreach (Treasure t in game.Treasures())
                {
                    if (game.Distance(t.Location, p.InitialLocation) < MinTresure)
                    {
                        MinTresure = game.Distance(t.Location, p.InitialLocation);
                        PiratesIndex = PiratesCount;
                        TresureIndex = TresureCount;
                    }
                    TresureCount++;
                }
                PiratesCount++;
            }
            this.Collector = Pirates[PiratesIndex];
            this.Target = game.Treasures()[TresureIndex];
            game.Debug("Collector = {0} Tresure = {1}", Collector.Id, Target.Id);
        }

    }

    #endregion

    #region Manngment

    class GameLogic
    {
        IPirateGame game;
        List<Pirate> MyPirates = new List<Pirate>();
        List<Pirate> EnemyPirates = new List<Pirate>();
        ColltionAvoider collitionAvoider = new ColltionAvoider();
        Crashing crash = null;

        int Moves = 6;

        public void Inint(IPirateGame Game)
        {
            this.game = Game;
            this.MyPirates = Game.MyPirates();
            this.EnemyPirates = Game.EnemyPirates();
            this.Moves = Game.GetActionsPerTurn();
            collitionAvoider.Game = game;
            collitionAvoider.Reset();
            collitionAvoider.Init(false);
        } //  Update the values each turn

        public void BasicStratgey(IPirateGame Game)
        {
            List<Pirate> UsedPirate = new List<Pirate>();


            Inint(Game);
            game.Debug("1");
            #region Crashing Test
            if (crash != null)
            {
                game.Debug("1.1");
                if (game.MyPirates().Contains(crash.Player))
                {
                    crash.game = game;
                    crash.DoTurn();
                    game.Debug("1.2");
                    if (crash.HasCompleted)
                    {
                        game.Debug("1.3");
                        this.Moves -= crash.Moves;
                        UsedPirate.Add(crash.Player);
                        game.Debug("1.4");
                    }
                }
                else
                {
                    game.Debug("1.5");
                    crash = null;
                }
            }
            #endregion
            game.Debug("2");
            #region Pirate With Tresures
            foreach (Pirate p in MyPirates) // Return home pirate with tresure;
            {
                if (p.HasTreasure)
                {
                    game.Debug("Pirate {0} Has Tresure", p.Id);
                    if (Moves > 0)
                    {
                        HomeSick HS = new HomeSick();
                        HS.CA = collitionAvoider;
                        HS.game = game;
                        HS.pirate = p;
                        HS.DoHomeTurn();
                        Moves -= p.CarryTreasureSpeed;
                        UsedPirate.Add(p);
                    }
                }
            }
            #endregion
            game.Debug("3");
            #region Attackers test
            List<Pirate> Targets = game.EnemyPiratesWithTreasures();
            List<Pirate> AllAttackers = game.MyPiratesWithoutTreasures();
            List<Pirate> PosibleAttackers = new List<Pirate>();
            game.Debug("3.1");
            foreach (Pirate p in AllAttackers)
            {
                if (p.ReloadTurns == 0 && !p.IsLost && p.TurnsToSober ==0 && p.TurnsToRevive ==0.)
                {
                    PosibleAttackers.Add(p);
                    
                }
            }

            game.Debug("3.2");
            
            if (Targets.Count > 0 && PosibleAttackers.Count > 0)
            {
                game.Debug("Has Targets and Posible Attackers");
                Attacking attacking = new Attacking();
                if (Moves > 0)
                {
                    game.Debug("3.3");
                    attacking.CA = collitionAvoider;
                    attacking.game = game;
                    attacking.FindBestMatch(PosibleAttackers, Targets);
                    game.Debug("3.4");
                    attacking.AttackerTurn();
                    if (attacking.hasmoves)
                    {
                        game.Debug("3.5");
                        UsedPirate.Add(attacking.Attacker);
                    }
                    Moves -= attacking.MovesLeft;
                    
                    if (attacking.HasShot)
                    {
                        game.Debug("3.6");
                       /* if (crash == null)
                        {
                            crash = new Crashing();
                            crash.Player = attacking.Attacker;
                            game.Debug("3.7");
                        }*/
                        return;

                    }
                }

            }

            #endregion
            game.Debug("4");
            #region Tresures Hunters

            if (Moves > 0)
            {
                game.Debug("4.1");
                List<Pirate> CurrentPirates = game.MyPirates();
                if (UsedPirate.Count != 0)
                {
                    foreach (Pirate p in UsedPirate)
                    {
                        CurrentPirates.Remove(p);
                    }
                }
                game.Debug("4.2");
                if (CurrentPirates != null)
                {
                    Collectors c = new Collectors();
                    c.Game = game;
                    c.CA = collitionAvoider;
                    game.Debug("4.2.1");
                    c.Findbest(CurrentPirates, game);
                    game.Debug("4.2.2");
                    c.Moves = Moves;
                    game.Debug("4.2.3");
                    c.DoTurn();
                    game.Debug("4.2.4");
                }
                game.Debug("4.3");
            }

            #endregion
            game.Debug("5");
        }

    }


    #endregion

}


