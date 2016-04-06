using System.Collections.Generic;
using System;
using Pirates;

namespace MyBot
{
    #region Enum
    public enum EnemyAction
    {
        Nothing = 0,
        DontKnow = 1,
        WillSail = 2,
        WillAtack = 3,
        WillDefence = 4
    }
    public enum TypeOfAction
    {
        SailHome = 0,
        AttackOrcrash = 1,
        GetTreasure = 2,
        Attack = 3,
        Defence = 4,
        GetPowerUp = 5,
        RunAway = 6,
        SaveHim = 7,
        Crash = 8
    }
    #endregion

    public class MyBot : Pirates.IPirateBot
    {
        private EnemyPredictor EP = new EnemyPredictor();
        public void DoTurn(IPirateGame game)
        {
            EP.AddTurn(game);

            List<Ship> ships = new List<Ship>();
            List<Actions> allActions = new List<Actions>();
            foreach (Pirate p in game.AllMyPirates())
            {
                Ship s = new Ship(game, p, EP);
                ships.Add(s);
                List<Actions> shipAllActions = s.getAllPossibleActions();
                allActions.AddRange(shipAllActions);
            }

            Strategy t1 = new Strategy();
            t1.init(game, allActions);
            t1.StrategyA();

            List<Actions> toDo = t1.ToDo;

            int numberOfturnsToDo = game.GetActionsPerTurn();
            foreach (Actions a in toDo)
            {
                if (numberOfturnsToDo == 0)
                    break;
                int actualMoves = a.DoIt(numberOfturnsToDo);
                numberOfturnsToDo -= actualMoves;
            }
        }
    }
    public class Ship
    {
        private Pirate Player;
        public Pirate player { get { return Player; } set { Player = value; } }

        private IPirateGame Game;
        private EnemyPredictor EP;

        public Ship(Pirate p)
        {
            Player = p;
        }

        public Ship(IPirateGame g, Pirate p, EnemyPredictor ep)
        {
            init(g, p, ep);
        }

        public void init(IPirateGame g, Pirate p, EnemyPredictor ep)
        {
            Game = g;
            Player = p;
            EP = ep;
        }

        public List<Actions> getAllPossibleActions()
        {
            List<Actions> allActions = new List<Actions>();

            if (Player.TurnsToRevive > 0 || Player.TurnsToSober > 0)
                return allActions;

            if (Player.DefenseReloadTurns == 0)
            {
                List<Actions> defenceAction = getDefenceAction();
                allActions.AddRange(defenceAction);
            }
            if (Player.ReloadTurns == 0 && !Player.HasTreasure)
            {
                List<Actions> AttackAction = getAttackAction();
                allActions.AddRange(AttackAction);
            }
            if (Player.HasTreasure)
            {
                List<Actions> SailHomeAction = getSailHomeAction();
                allActions.AddRange(SailHomeAction);
            }
            if (!Player.HasTreasure)
            {
                List<Actions> GetTreasureAction = getDefenceAction();
                allActions.AddRange(GetTreasureAction);
                List<Actions> AttackOrCrashAction = getAttackOrCrashAction();
                allActions.AddRange(AttackOrCrashAction);
                List<Actions> CrashAction = getCrashAction();
                allActions.AddRange(CrashAction);
            }

            /* List<Actions> PowerUpsAction = getPowerUpsAction();
               List<Actions> RunAwayAction = getRunAwayActions();
               List<Actions> SaveHimAction = getSaveHimAction();
               allActions.AddRange(SaveHimAction);
               allActions.AddRange(RunAwayAction);
               allActions.AddRange(PowerUpsAction);*/

            return allActions;
        }

        private List<Actions> getCrashAction()
        {
            List<Actions> Crash = new List<Actions>();

            foreach (Pirate e in Game.EnemyPirates())
            {
                if (e.TurnsToSober == 0)
                {
                    Crash.Add(new ActionCrash(Game, Player, e, EP));
                }
            }

            return Crash;
        }
        private List<Actions> getSaveHimAction()
        {
            List<Actions> SaveHimAction = new List<Actions>();

            foreach (Pirate p in Game.MyPirates())
            {
                foreach (Pirate mdp in Game.MyDrunkPirates())
                {
                    SaveHimAction.Add(new ActionSaveHim(Game, p, EP, mdp));
                }
            }
            return SaveHimAction;
        }
        private List<Actions> getRunAwayActions()
        {
            List<Actions> RunAwayAction = new List<Actions>();
            foreach (Pirate p in Game.MyPirates())
            {
                RunAwayAction.Add(new ActionRunAway(Game, p, EP));
            }
            return RunAwayAction;

        }
        private List<Actions> getPowerUpsAction()
        {
            List<Actions> PowerUpsAction = new List<Actions>();
            foreach (Pirate p in Game.MyPiratesWithoutTreasures())
            {
                foreach (Powerup u in Game.Powerups())
                {
                    PowerUpsAction.Add(new ActionGetPowerUp(Game, p, EP, u));
                }
            }
            return PowerUpsAction;
        }
        private List<Actions> getAttackOrCrashAction()
        {
            List<Actions> AttackOrCrashAction = new List<Actions>();

            foreach (Pirate p in Game.MyPiratesWithoutTreasures())
            {
                foreach (Pirate e in Game.EnemyPirates())
                {
                    if (e.TurnsToSober == 0)
                        AttackOrCrashAction.Add(new ActionAttackOrcrash(Game, p, e, EP));
                }
            }
            return AttackOrCrashAction;
        }
        private List<Actions> getTreasureAction()
        {
            List<Actions> TresureActions = new List<Actions>();
            foreach (Pirate p in Game.MyPiratesWithoutTreasures())
            {
                foreach (Treasure t in Game.Treasures())
                    TresureActions.Add(new ActionGetTreasure(Game, p, EP, t));
            }
            return TresureActions;
        }
        private List<Actions> getDefenceAction()
        {
            List<Actions> defenceActions = new List<Actions>();
            foreach (Pirate e in Game.EnemyPiratesWithoutTreasures())
            {
                if (EP.GetEnemyNextAction(e) == EnemyAction.WillAtack || EP.GetEnemyNextAction(e) == EnemyAction.DontKnow)
                {
                    if (Game.Distance(Player, e) <= e.AttackRadius && Player.DefenseReloadTurns == 0)
                    {
                        defenceActions.Add(new ActionDefence(Game, Player, EP));
                    }
                }
            }
            return defenceActions;
        }
        private List<Actions> getAttackAction()
        {
            List<Actions> AttackActions = new List<Actions>();
            foreach (Pirate e in Game.EnemyPirates())
            {
                if (!(EP.GetEnemyNextAction(e) == EnemyAction.WillDefence))
                {
                    if (e.TurnsToSober > 0)
                        continue;
                    AttackActions.Add(new ActionAttack(Game, Player, e, EP));
                }
            }
            return AttackActions;
        }
        private List<Actions> getSailHomeAction()
        {
            List<Actions> SailHomeAction = new List<Actions>();
            foreach (Pirate p in Game.MyPiratesWithTreasures())
            {
                SailHomeAction.Add(new ActionSailHome(Game, p, EP));
            }
            return SailHomeAction;
        }
    }
    public class Strategy
    {
        private IPirateGame Game;
        private List<Actions> AllPossibleAction = new List<Actions>();
        private List<Actions> Strategy1 = new List<Actions>();
        public List<Actions> ToDo = new List<Actions>();

        CollisionAvoider CA;

        public void init(IPirateGame g, List<Actions> APA)
        {
            Game = g;
            AllPossibleAction = APA;
        }

        public void StrategyA()
        {
            #region General Instructions
            /*
             ######Steps######
             * 1) Protect vulnerable pirate ship
             * 2) 
             * 3) 
             * 4) if tresure is better - Calculate  how many moves will take (if enemy has tresure) kill enemy, or go and get tresure 
             * 5) Turn Conclusion 
             * 6) Do the turn, with enemy predictor and etc.
                     
             ###### Notice and special conditions ######
             * A) Enemy leading
             * B) We leading
             * C) Game will be  over soon
             * D) Lost and drunk pirates enemy and mine
             * E) Collision avoider !
             * F) Powerup Has spawned on map
             */
            #endregion

            /*
                        List<Actions> GetTreasures = T1_AddGetTreasuresAction(AllPossibleAction);
                        //     GetTreasures = EveryPirateOne(GetTreasures);

                        List<Actions> GetDefence = T1_AddDefence(AllPossibleAction);

                        List<Actions> SailHome = T1_SailHome(AllPossibleAction);

                        List<Actions> Attack = T1_AddAttack(AllPossibleAction);


                        List<Ship> UsedShips = new List<Ship>();
                        List<Pirate> MyPiratedWithTreasure = new List<Pirate>();
                    */

            List<Pirate> UsedShips = new List<Pirate>();
            List<Pirate> MyPiratedWithTreasure = new List<Pirate>();

            int Moves = Game.GetActionsPerTurn();
            CA = new CollisionAvoider(Game);
            CA.init(true, true, true);
            CA.Reset();

            #region GoHome
            foreach (Pirate p in MyPiratedWithTreasure)
            {
                if (Moves > 0)
                {
                    // return shipHome
                    Location ToGo = CA.TryAdd(Game.GetSailOptions(p, p.InitialLocation, p.CarryTreasureSpeed));
                    if (ToGo != null)
                    {
                        Game.Debug("Pirate {0} will try to go home, Turns till home ={1}", p.Id, StepsTillHome(new Ship(p)));
                        Game.SetSail(p, ToGo);
                        Moves -= p.CarryTreasureSpeed;
                    }
                    else
                    {
                        Game.Debug("Error.. Pirate {0} has no way to go home", p.Id);
                    }
                }
            }
            #endregion

            if (Moves > 0 && Game.EnemyPiratesWithTreasures().Count > 1)
            {
                #region Attacking
                List<Pirate> EnemyTargets = Game.EnemyPiratesWithTreasures();
                List<Pirate> MyOptionToAttack = Game.MyPiratesWithoutTreasures();
                List<Pirate> MyAttackers = new List<Pirate>();

                foreach (Pirate p in MyOptionToAttack)
                {
                    if (!p.IsLost && p.TurnsToSober == 0)
                    {
                        MyAttackers.Add(p);
                    }
                }

                ActionRegularAttack ARA = new ActionRegularAttack();
                ARA.FindBestMatch(MyAttackers, EnemyTargets);
                ARA.game = Game;
                ARA.CA = CA;
                if (ARA.Attacker != null && ARA.Target != null)
                {
                    ARA.MovesLeft = Moves - Game.MyPiratesWithTreasures().Count;
                    ARA.DoTurn();
                    if (ARA.hasmoves)
                    {
                        UsedShips.Add(ARA.Attacker);
                        Moves = 0;
                    }
                }


                #endregion
            }


            #region Collectors
            if (Game.Treasures().Count > 0)
            {
                List<Pirate> CanGetTresure = Game.MyPiratesWithoutTreasures();
                List<Pirate> WillGoAndGet = new List<Pirate>();
                foreach (Pirate p in CanGetTresure)
                {
                    if (p.TurnsToSober == 0 && !p.IsLost)
                    {
                        WillGoAndGet.Add(p);
                    }
                }
                if (WillGoAndGet.Count > 0)
                {
                    ActionRegularGetTreasure argt = new ActionRegularGetTreasure();

                    argt.Game = Game;
                    argt.CA = CA;
                    argt.FindBest(WillGoAndGet);
                    argt.Moves = Moves;
                    try
                    {
                        argt.DoTurn();
                    }
                    catch (Exception ex)
                    {
                        Game.Debug("Exception {0}", ex);
                    }
                    UsedShips.Add(argt.Collector);
                    Moves -= argt.Moves;

                }


            }
            #endregion

            #region Wasted Startgey 

            #endregion

            /*
            List<Actions> AttackOrcrash = T1_AddAttackOrcrash(AllPossibleAction);
            Attack = EveryPirateOne(Attack);
            List<Actions> GetPowerUp = T1_GetPowerUpsAction(AllPossibleAction);
            GetTreasures = EveryPirateOne(GetTreasures);
            List<Actions> BestPossibleActions = new List<Actions>();
            */


        }

        /*private List<Actions> T1_GetPowerUpsAction(List<Actions> AllPossibleAction)
        {
          
        }*/
        #region Math
        private int StepsTillHome(Ship s)
        {
            return (Game.Distance(s.player.Location, s.player.InitialLocation) / s.player.CarryTreasureSpeed);
        }

        private int CanReachPirateWithTreasure(Ship enemy, Ship mine)
        {
            int StepTillHomeEnemy = StepsTillHome(enemy);
            int StepsTillMineReach = Game.Distance(enemy.player.Location, mine.player.Location) / (Game.GetActionsPerTurn() - Game.MyPiratesWithTreasures().Count);
            if (StepTillHomeEnemy > StepsTillMineReach)
            {
                return StepsTillMineReach;
            }
            return -1;
        }

        private int CanReachLocationBeforeEnemy(Location loc, Ship Enemy, Ship Mine)
        {
            int EnemyScore = StepTillLocationMaxSpeed(Enemy, loc);
            int MyScore = (Game.Distance(Mine.player.Location, loc) / (Game.GetActionsPerTurn() - Game.MyPiratesWithTreasures().Count));
            if (EnemyScore > MyScore)
            {
                return MyScore;
            }
            return -1;
        }
        private int StepTillLocationMaxSpeed(Ship s, Location loc)
        {
            return Game.Distance(s.player.Location, loc) / Game.GetActionsPerTurn();
        }
        #endregion




        private List<Actions> T1_AddDefence(List<Actions> AllPossibleAction)
        {
            List<Actions> Defence = new List<Actions>();
            foreach (Actions a in AllPossibleAction)
            {
                ActionDefence CurA = (ActionDefence)a;
                if (CurA.ActionType == TypeOfAction.Defence)
                {
                    Defence.Add(a);
                }
            }
            return Defence;
        }
        private List<Actions> T1_AddGetTreasuresAction(List<Actions> AllPossibleAction)
        {
            List<Actions> T = new List<Actions>();
            if (AllPossibleAction.Count == 0)
                return T;
            foreach (Actions a in AllPossibleAction)
            {
                if (a.ActionType == TypeOfAction.GetTreasure)
                    T.Add(a);
            }
            double BestDist = -1000;
            List<Actions> bestActions = new List<Actions>();
            Actions curBestAction = T[0];
            int prevId = -1;
            foreach (Actions a in T)
            {
                if (a.Player.Id != prevId)
                {
                    if (BestDist > -1)
                        bestActions.Add(curBestAction);
                    BestDist = -1000;
                }
                ActionGetTreasure curAc = (ActionGetTreasure)a;
                double ScoreDist = curAc.Score();
                if (ScoreDist > BestDist || (ScoreDist == BestDist && new Random().Next(0, 2) == 0))
                {
                    curBestAction = a;
                    BestDist = ScoreDist;
                }
                prevId = a.Player.Id;
            }

            return bestActions;
        }

        private List<Actions> T1_AddGetPowerUp(List<Actions> AllPossibleAction)
        {
            List<Actions> PU = new List<Actions>();
            foreach (Actions a in AllPossibleAction)
            {
                if (a.ActionType == TypeOfAction.GetPowerUp)
                    PU.Add(a);
            }
            int minDist = 1000;
            List<Actions> bestActions = new List<Actions>();
            foreach (Actions a in PU)
            {
                ActionGetPowerUp curAc = (ActionGetPowerUp)a;
                int curDist = curAc.HowManySteps();
                if (curDist < minDist)
                {
                    bestActions.Add(a);
                    minDist = curDist;
                }
            }

            return bestActions;
        }

        private List<Actions> T1_SailHome(List<Actions> AllPossibleAction)
        {
            List<Actions> SH = new List<Actions>();
            foreach (Actions a in AllPossibleAction)
            {
                if (a.ActionType == TypeOfAction.SailHome)
                {
                    SH.Add(a);
                }
            }
            return SH;
        }


        private List<Actions> T1_AddAttacks(List<Actions> AllPossibleAction)
        {
            List<Actions> AllAttacks = new List<Actions>();
            List<Actions> BestAttacks1 = new List<Actions>();
            List<Actions> BestAttacks2 = new List<Actions>();
            List<Actions> BestAttacks3 = new List<Actions>();
            foreach (Actions a in AllPossibleAction)
            {
                if (a.ActionType == TypeOfAction.Attack)
                    AllAttacks.Add(a);
            }

            foreach (Actions ToCheck in AllAttacks)
            {
                ActionAttack AttackToCheck = (ActionAttack)ToCheck;
                if (AttackToCheck.ENEMY.HasTreasure)
                {
                    Location NextLocation = Game.GetSailOptions(AttackToCheck.ENEMY, AttackToCheck.ENEMY.InitialLocation, AttackToCheck.ENEMY.CarryTreasureSpeed)[0];
                    if (Game.InRange(AttackToCheck.Player, NextLocation))
                    {
                        BestAttacks1.Add(AttackToCheck);
                    }
                    else
                    {
                        BestAttacks2.Add(AttackToCheck);
                    }
                }
                else
                {
                    BestAttacks3.Add(AttackToCheck);
                }
            }

            if (BestAttacks1.Count > 0)
            {
                int ID = BestAttacks1[0].Player.Id;
                foreach (Actions a in BestAttacks1)
                {
                    if (a.Player.Id != ID)
                    {
                        ID = a.Player.Id;

                    }
                }
            }
            return null;


        }

        private List<Actions> T1_AddAttack(List<Actions> AllPossibleAction)
        {
            List<Actions> allAttacks = new List<Actions>();
            foreach (Actions a in AllPossibleAction)
            {
                if (a.ActionType == TypeOfAction.Attack)
                {
                    allAttacks.Add(a);
                }
            }
            List<Actions> AT2 = new List<Actions>();
            List<Actions> AT3 = new List<Actions>();
            List<Actions> AT4 = new List<Actions>();
            List<Actions> BestWayToAttack = new List<Actions>();
            List<Actions> BestWayToAttackChecker = new List<Actions>();
            List<Actions> BestWayToAttackChecker2 = new List<Actions>();
            int min1 = 1000;
            int min2 = 1000;
            int min3 = 1000;
            foreach (Actions a in allAttacks)
            {
                ActionAttack CurA = (ActionAttack)a;
                if (CurA.ENEMY.HasTreasure)
                {
                    if (CurA.attack() == -1)
                    {
                        BestWayToAttack.Add(a);
                    }
                    if (Game.Distance(CurA.ENEMY, CurA.ENEMY.InitialLocation) <= min1)
                    {
                        if (Game.Distance(CurA.ENEMY, CurA.ENEMY.InitialLocation) < min1)
                            AT2.Clear();
                        AT2.Add(a);
                        min1 = Game.Distance(CurA.ENEMY, CurA.ENEMY.InitialLocation);
                    }
                    if (CurA.attack() <= min2)
                    {
                        if (CurA.attack() < min2)
                            AT3.Clear();
                        AT3.Add(a);
                        min2 = CurA.attack();
                    }
                }
                else
                {
                    if (CurA.attack() <= min3)
                    {
                        if (CurA.attack() < min3)
                            AT4.Clear();
                        AT4.Add(a);
                        min3 = CurA.attack();
                    }
                }
            }
            foreach (Actions a in AT2)
            {
                bool Check = true;
                foreach (Actions b in BestWayToAttack)
                {
                    if (b.Player == a.Player)
                    {
                        Check = false;
                    }
                }
                if (Check)
                {
                    BestWayToAttack.Add(a);
                }
            }
            foreach (Actions a in AT3)
            {
                bool Check = true;
                foreach (Actions b in BestWayToAttack)
                {
                    if (b.Player == a.Player)
                    {
                        Check = false;
                    }
                }
                if (Check)
                {
                    BestWayToAttack.Add(a);
                }
            }
            foreach (Actions a in AT4)
            {
                bool Check = true;
                foreach (Actions b in BestWayToAttack)
                {
                    if (b.Player == a.Player)
                    {
                        Check = false;
                    }
                }
                if (Check)
                {
                    BestWayToAttack.Add(a);
                }
            }
            return BestWayToAttack;
        }

        private List<Actions> T1_AddAttackOrcrash(List<Actions> AllPossibleAction)
        {
            List<Actions> AT1 = new List<Actions>();
            List<Actions> AT2 = new List<Actions>();
            List<Actions> AT3 = new List<Actions>();
            List<Actions> AT4 = new List<Actions>();
            List<Actions> BestWayToAttack = new List<Actions>();
            List<Actions> BestWayToAttackChecker = new List<Actions>();
            List<Actions> BestWayToAttackChecker2 = new List<Actions>();
            foreach (Actions a in AllPossibleAction)
            {
                if (a.ActionType == TypeOfAction.AttackOrcrash)
                {
                    AT1.Add(a);
                }
            }
            int min1 = 1000;
            int min2 = 1000;
            int min3 = 1000;
            foreach (Actions a in AT1)
            {
                ActionAttackOrcrash CurA = (ActionAttackOrcrash)a;
                if (CurA.ENEMY.HasTreasure)
                {
                    if (CurA.attack() == -1)
                    {
                        BestWayToAttack.Add(a);
                    }
                    if (Game.Distance(CurA.ENEMY, CurA.ENEMY.InitialLocation) <= min1)
                    {
                        if (Game.Distance(CurA.ENEMY, CurA.ENEMY.InitialLocation) < min1)
                            AT2.Clear();
                        AT2.Add(a);
                        min1 = Game.Distance(CurA.ENEMY, CurA.ENEMY.InitialLocation);
                    }
                    if (CurA.attack() <= min2)
                    {
                        if (CurA.attack() < min2)
                            AT3.Clear();
                        AT3.Add(a);
                        min2 = CurA.attack();
                    }
                }
                else
                {
                    if (CurA.attack() <= min3)
                    {
                        if (CurA.attack() < min3)
                            AT4.Clear();
                        AT4.Add(a);
                        min3 = CurA.attack();
                    }
                }
            }
            foreach (Actions a in AT2)
            {
                bool Check = true;
                foreach (Actions b in BestWayToAttack)
                {
                    if (b.Player == a.Player)
                    {
                        Check = false;
                    }
                }
                if (Check)
                {
                    BestWayToAttack.Add(a);
                }
            }
            foreach (Actions a in AT3)
            {
                bool Check = true;
                foreach (Actions b in BestWayToAttack)
                {
                    if (b.Player == a.Player)
                    {
                        Check = false;
                    }
                }
                if (Check)
                {
                    BestWayToAttack.Add(a);
                }
            }
            foreach (Actions a in AT4)
            {
                bool Check = true;
                foreach (Actions b in BestWayToAttack)
                {
                    if (b.Player == a.Player)
                    {
                        Check = false;
                    }
                }
                if (Check)
                {
                    BestWayToAttack.Add(a);
                }
            }
            return BestWayToAttack;
        }

        private void removeAllActionsOfShip(Pirate p)
        {
            foreach (Actions a in AllPossibleAction)
            {
                if (a.Player == p)
                    AllPossibleAction.Remove(a);
            }
        }

        private List<Actions> EveryPirateOne(List<Actions> LA)
        {
            List<Actions> ActionPirateOne = new List<Actions>();
            while (LA.Count != 0)
            {
                ActionPirateOne.Add(LA[0]);
                foreach (Actions a in ActionPirateOne)
                {
                    LA = removeAllActionsOfShip(a.Player, LA);
                }
            }
            return ActionPirateOne;
        }

        private List<Actions> removeAllActionsOfShip(Pirate p, List<Actions> A)
        {
            foreach (Actions a in A)
            {
                if (a.Player == p)
                    A.Remove(a);
            }
            return A;
        }

    }

    #region Moves
    public class EnemyPredictor
    {
        class GameState
        {
            private IPirateGame game;
            public IPirateGame Game { get { return this.game; } set { this.game = value; } }

            private List<Ship> myShips;
            public List<Ship> MyShips { get { return this.myShips; } set { this.myShips = value; } }

            private List<Ship> enemyShips;
            public List<Ship> EnemyShips { get { return this.enemyShips; } set { this.enemyShips = value; } }

            private List<Treasure> allTreasures;
            public List<Treasure> AllTreasures { get { return this.allTreasures; } set { this.allTreasures = value; } }

            private List<Powerup> allPowerUps;
            public List<Powerup> AllPowerups { get { return this.allPowerUps; } set { this.allPowerUps = value; } }

            public GameState(IPirateGame g)
            {
                Game = g;
                myShips = new List<Ship>();
                enemyShips = new List<Ship>();
                allTreasures = new List<Treasure>();
                allPowerUps = new List<Powerup>();

                foreach (Pirate P in g.AllMyPirates())
                {
                    Pirate p = new Pirate(P.Id, P.Owner, P.Location, P.InitialLocation, P.AttackRadius);
                    p.CarryTreasureSpeed = P.CarryTreasureSpeed;
                    p.DefenseExpirationTurns = P.DefenseExpirationTurns;
                    p.DefenseReloadTurns = P.DefenseReloadTurns;
                    p.HasTreasure = P.HasTreasure;
                    p.IsLost = P.IsLost;
                    p.Powerups = P.Powerups;
                    p.ReloadTurns = P.ReloadTurns;
                    p.TreasureValue = P.TreasureValue;
                    p.TurnsToRevive = P.TurnsToRevive;
                    p.TurnsToSober = P.TurnsToSober;
                    Ship s = new Ship(p);
                    MyShips.Add(s);
                }

                foreach (Pirate E in g.AllEnemyPirates())
                {
                    Pirate e = new Pirate(E.Id, E.Owner, E.Location, E.InitialLocation, E.AttackRadius);
                    e.CarryTreasureSpeed = E.CarryTreasureSpeed;
                    e.DefenseExpirationTurns = E.DefenseExpirationTurns;
                    e.DefenseReloadTurns = E.DefenseReloadTurns;
                    e.HasTreasure = E.HasTreasure;
                    e.IsLost = E.IsLost;
                    e.Powerups = E.Powerups;
                    e.ReloadTurns = E.ReloadTurns;
                    e.TreasureValue = E.TreasureValue;
                    e.TurnsToRevive = E.TurnsToRevive;
                    e.TurnsToSober = E.TurnsToSober;
                    Ship s = new Ship(e);
                    EnemyShips.Add(s);
                }

                foreach (Treasure T in g.Treasures())
                {
                    Treasure t = new Treasure(T.Id, T.Location, T.Value);
                    AllTreasures.Add(t);
                }

                foreach (Powerup PU in g.Powerups())
                {
                    Powerup pu = new Powerup(PU.Id, PU.Type, PU.Location, PU.ActiveTurns, PU.EndTurn);
                    //    AllPowerUps.Add(pu);
                }
            }

        }
        private List<GameState> AllTurns;
        private IPirateGame Game;

        public EnemyPredictor()
        {
            AllTurns = new List<GameState>();
        }
        public void AddTurn(IPirateGame g)
        {
            Game = g;
            AllTurns.Add(new GameState(g));
        }
        public Location GetEnemyNextLocation(Pirate e)
        {
            // calculate what this enemy did and what do you think he will do next
            // FOR NOW return his current location or his best move if he has a treasure.
            if (e.HasTreasure)
            {
                List<Location> EnemyWay = Game.GetSailOptions(e, e.InitialLocation, e.CarryTreasureSpeed);
                if (EnemyWay.Count == 0)
                    return e.Location;
                else
                    return EnemyWay[0];
            }
            else
            {
                if (GetEnemyNextAction(e) == EnemyAction.WillSail)
                {
                    return WhatDoYouThinkEnemyWillDo();
                }
                return e.Location;
            }
        }
        public EnemyAction GetEnemyNextAction(Pirate e)
        {
            // FOR NOW the enemy allways sail, later check earlier turns and try to understand what is going on.
            EnemyAction ac = EnemyAction.WillSail;
            if (e.TurnsToRevive > 0 || e.TurnsToSober > 0)
                ac = EnemyAction.Nothing;
            if (IsGoingToDefence(e))
                ac = EnemyAction.WillDefence;
            if (IsGoingToAttack(e))
                ac = EnemyAction.WillAtack;
            return ac;
        }
        private bool IsGoingToAttack(Pirate e)
        {
            if (e.ReloadTurns == 0 && !e.IsLost && e.TurnsToSober == 0)
            {
                foreach (Pirate p in Game.MyPiratesWithTreasures())
                {
                    if (Game.InRange(e, p))
                        return true;
                }
            }
            return false;
        }
        private bool IsGoingToDefence(Pirate e)
        {
            foreach (Pirate p in Game.MyPiratesWithoutTreasures())
            {
                if (p.ReloadTurns == 0 && Game.InRange(p, e))
                {
                    return true;
                }
            }
            return false;
        }
        private Location WhatDoYouThinkEnemyWillDo()
        {
            if (AllTurns.Count < 3)
            {
                return null;
            }
            GameState First = AllTurns[AllTurns.Count - 3];
            GameState Second = AllTurns[AllTurns.Count - 2];
            GameState Third = AllTurns[AllTurns.Count - 1];

            return null;
        }

        //private List<Actions> WhatEnemyWillDo()
        //{


        //  }
        private void HowEnemyAttacks()
        {
            GameState TurnBeforeHeAttacked = AllTurns[AllTurns.Count - 2];
            List<Ship> EnemyAbleToShoot = new List<Ship>();
            List<Ship> MyAvailble = AllTurns[AllTurns.Count - 1].MyShips;
            List<Ship> PiratesShooted = new List<Ship>();
            List<Ship> EnemyAbleToShoot1 = WhoCanShootInTurn(TurnBeforeHeAttacked);
            List<Ship> EnemyAbleToShoot2 = WhoCanShootInTurn(AllTurns[AllTurns.Count - 1]);
            Dictionary<Ship, Ship> Paires = new Dictionary<Ship, Ship>();


            if (EnemyAbleToShoot1.Count > 0)
            {
                PiratesShooted = WhoShooted(EnemyAbleToShoot1, EnemyAbleToShoot2);
            }

            if (PiratesShooted.Count > 0)
            {
                Paires = WhoGotShot(PiratesShooted, MyAvailble);
                if (Paires.Count > 0)
                {
                    for (int i = 0; i < Paires.Count; i++)
                    {
                        if (Paires[PiratesShooted[i]].player.DefenseReloadTurns > 0)
                        {

                        }
                    }
                }
            }

        }
        private List<Ship> WhoShooted(List<Ship> P1, List<Ship> P2)
        {
            List<Ship> Shoots = new List<Ship>();
            foreach (Ship e in P1)
            {
                if (P2.Contains(e))
                {
                    continue;
                }
                Shoots.Add(e);
            }
            return Shoots;
        }
        private List<Ship> WhoCanShootInTurn(GameState TurnBeforeHeAttacked)
        {
            List<Ship> CanShoot = new List<Ship>();
            foreach (Ship e in TurnBeforeHeAttacked.EnemyShips)
            {
                if (e.player.TurnsToSober == 0 && e.player.ReloadTurns == 0 && !e.player.HasTreasure)
                    CanShoot.Add(e);
            }
            return CanShoot;
        }
        private Dictionary<Ship, Ship> WhoGotShot(List<Ship> PiratesShooted, List<Ship> MyPirates)
        {
            Dictionary<Ship, Ship> GotShot = new Dictionary<Ship, Ship>();
            foreach (Ship e in PiratesShooted)
            {
                foreach (Ship p in MyPirates)
                {
                    if (Game.InRange(e.player, p.player))
                    {
                        if (p.player.TurnsToSober == Game.GetSoberTurns() || p.player.DefenseExpirationTurns == Game.GetDefenseExpirationTurns()) //Check if -1
                        {
                            GotShot.Add(e, p);
                            continue;
                        }
                    }
                }
            }
            return GotShot;
        }

    }
    public class CollisionAvoider
    {
        private List<Location> CaptureLocation = new List<Location>();

        private IPirateGame Game;
        private bool WithTreasures = false;
        private bool MyDrunkPirates = true;
        private bool Enemy = true;

        public CollisionAvoider(IPirateGame g)
        {
            Game = g;
        }

        public void init(bool WT, bool MDP, bool E)
        {
            WithTreasures = WT;
            MyDrunkPirates = MDP;
            Enemy = E;
            init();
        }

        public void init()
        {
            Reset();
            InitAvoider();
        }

        public void Reset()
        {
            this.CaptureLocation.Clear();
        }

        public void InitAvoider()
        {
            if (MyDrunkPirates && Game.MyDrunkPirates() != null)
                foreach (Pirate p in Game.MyDrunkPirates()) // ספינות שלי שיכורות
                {
                    CaptureLocation.Add(p.Location);
                }
            if (Enemy)
            {
                if (Game.EnemyPiratesWithTreasures() != null)
                    foreach (Pirate e in Game.EnemyPiratesWithTreasures()) // ספינות אוייב עם אוצר
                    {
                        List<Location> ETW = Game.GetSailOptions(e, e.InitialLocation, e.CarryTreasureSpeed);
                        foreach (Location l in ETW)
                        {
                            CaptureLocation.Add(l);
                        }
                    }
                if (Game.EnemyPirates() != null)
                {
                    foreach (Pirate e in Game.EnemyPirates())
                    {
                        CaptureLocation.Add(e.Location);
                    }
                }
            }
            if (Game.MyPirates() != null)
            {
                foreach (Pirate p in Game.MyPirates()) // ספינות שלי 
                {
                    if (p.TurnsToSober == 0)
                    {
                        CaptureLocation.Add(p.Location);
                    }
                }
            }

            if (WithTreasures)
            {
                foreach (Treasure T in Game.Treasures())
                {
                    CaptureLocation.Add(T.Location);
                }
            }
        }

        public bool CheckLocation(Location l)
        {
            if (CaptureLocation.Contains(l))
                return false;
            return true;
        }

        public Location TryAdd(List<Location> PossibleLocations)
        {
            List<Location> Possible = new List<Location>();
            Random rnd = new Random();

            foreach (Location l in PossibleLocations)
            {
                if (!CaptureLocation.Contains(l))
                {
                    Possible.Add(l);
                    //game.Debug("Location Added {0}-{1}", l.Row, l.Col);
                }
            }
            if (Possible.Count == 0)
            {
                //game.Debug("Did not Found Solution");
                return null;
            }
            int num = rnd.Next(0, Possible.Count);
            return Possible[num];
        }
    }
    #endregion

    #region Actions
    public class Actions
    {
        protected CollisionAvoider CA;
        protected IPirateGame Game;
        public Pirate Player;
        protected EnemyPredictor EP;
        protected TypeOfAction Action_type;
        public TypeOfAction ActionType { get { return Action_type; } protected set { Action_type = value; } }

        public Actions(IPirateGame g, Pirate p, EnemyPredictor ep)
        {
            Game = g;
            Player = p;
            EP = ep;
            CA = new CollisionAvoider(Game);
        }

        public virtual int DoIt(int a)
        {
            Game.Debug("Never get here");
            return 0;
        }
    }
    public class ActionSailHome : Actions
    {
        private int PossibleMoves;
        private int NeededMoves;

        public ActionSailHome(IPirateGame g, Pirate p, EnemyPredictor ep)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.SailHome;
        }

        public int HowManySteps()
        {
            if (Game.Distance(Player, Player.InitialLocation) < Player.CarryTreasureSpeed)
            {
                NeededMoves = Game.Distance(Player, Player.InitialLocation);
                return NeededMoves;
            }
            NeededMoves = Player.CarryTreasureSpeed;
            return NeededMoves;
        }

        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            CA.init();
            if (PossibleMoves >= NeededMoves)
            {
                Location l = CA.TryAdd(Game.GetSailOptions(Player, Player.InitialLocation, NeededMoves));
                Game.SetSail(Player, l);
                return NeededMoves;
            }
            else
            {
                Location l = CA.TryAdd(Game.GetSailOptions(Player, Player.InitialLocation, PossibleMoves));
                Game.SetSail(Player, l);
                return PossibleMoves;
            }
        }
    }
    public class ActionAttackOrcrash : Actions
    {
        private Pirate Enemy;
        private int PossibleMoves;
        private int NeededMoves;
        private bool CrashNow = true;
        private bool ShootNow = true;

        public ActionAttackOrcrash(IPirateGame g, Pirate p, Pirate e, EnemyPredictor ep)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.AttackOrcrash;
            init(e);
        }

        public void init(Pirate e)
        {
            Enemy = e;
        }

        public Pirate ENEMY { get { return Enemy; } set { Enemy = value; } }

        public int attack()
        {
            if (Enemy.DefenseExpirationTurns > 0 || Player.ReloadTurns > 0)
            {
                return crash();
            }
            else
            {
                return shoot();
            }
        }

        public int crash()
        {
            Location EnemyLocation = EP.GetEnemyNextLocation(Enemy);
            if (Game.Distance(Player, EnemyLocation) > Game.GetActionsPerTurn())
            {
                CrashNow = false;
            }
            NeededMoves = Game.Distance(Player, EnemyLocation);
            return NeededMoves;
        }

        public int shoot()
        {
            Location EnemyLocation = EP.GetEnemyNextLocation(Enemy);
            if (Game.Distance(Player, EnemyLocation) > Player.AttackRadius)
            {
                ShootNow = false;
                return crash();
            }
            return -1;
        }

        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            if (ShootNow)
            {
                Game.Attack(Player, Enemy);
                return PossibleMoves;
            }
            if (PossibleMoves >= NeededMoves && CrashNow)
            {
                Game.SetSail(Player, EP.GetEnemyNextLocation(Enemy));
                return NeededMoves;
            }
            else
            {
                CA.init(true, true, true);
                Location l = CA.TryAdd(Game.GetSailOptions(Player, Enemy, PossibleMoves));
                Game.SetSail(Player, l);
                return PossibleMoves;
            }
        }


    }
    public class ActionGetTreasure : Actions
    {
        private int PossibleMoves = 0;
        private int NeededMoves;
        private Treasure SelectedTreasure;

        public ActionGetTreasure(IPirateGame g, Pirate p, EnemyPredictor ep, Treasure T)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.GetTreasure;
            init(T);
        }

        public void init(Treasure T)
        {
            SelectedTreasure = T;
        }

        public int HowManySteps()
        {
            NeededMoves = Game.Distance(Player, SelectedTreasure);
            return NeededMoves;
        }
        public double Score()
        {
            //Value , Tresure Distance From House , Tresure Distase From Player
            double Score = (Game.Distance(Player.Location, SelectedTreasure.Location) / 6 + Game.Distance(Player.InitialLocation, SelectedTreasure.Location)) / SelectedTreasure.Value;
            Game.Debug("Pirate {0} with Tresure {1} has {3} Score ", Player.Id, SelectedTreasure.Id, Score);
            return Score;

        }


        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            if (PossibleMoves >= NeededMoves)
            {
                CA.init();
                Location l = CA.TryAdd(Game.GetSailOptions(Player, SelectedTreasure, NeededMoves));
                Game.SetSail(Player, l);
                return NeededMoves;
            }
            else
            {
                CA.init(true, true, true);
                Location l = CA.TryAdd(Game.GetSailOptions(Player, SelectedTreasure, PossibleMoves));
                Game.SetSail(Player, l);
                return PossibleMoves;
            }
        }
    }
    public class ActionAttack : Actions
    {
        private Pirate Enemy;
        private int PossibleMoves;
        private int NeededMoves;

        public ActionAttack(IPirateGame g, Pirate p, Pirate e, EnemyPredictor ep)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.Attack;
            init(e);
        }

        public void init(Pirate e)
        {
            Enemy = e;
        }

        public Pirate ENEMY { get { return Enemy; } set { Enemy = value; } }

        public int attack()
        {
            if (Game.Distance(Player, EP.GetEnemyNextLocation(Enemy)) <= Player.AttackRadius)
            {
                NeededMoves = -1;
                return -1;
            }
            NeededMoves = Game.Distance(Player, Enemy);
            return NeededMoves;
        }

        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            if (attack() < 0)
            {
                Game.Attack(Player, Enemy);
            }
            else
            {
                CA.init(true, true, true);
                Location l = CA.TryAdd(Game.GetSailOptions(Player, Enemy, PossibleMoves));
                Game.SetSail(Player, l);
            }
            return PossibleMoves;
        }

        public double score()
        {
            double Score = 0;
            if (Enemy.HasTreasure)
            {
                Score = ((Game.Distance(EP.GetEnemyNextLocation(Enemy), Enemy.InitialLocation) + (Game.Distance(Player, EP.GetEnemyNextLocation(Enemy)) / 6)) / Enemy.TreasureValue);
            }
            else
            {
                if (Enemy.ReloadTurns == 0)
                { }
            }
            return 0;
        }

    }
    public class ActionDefence : Actions
    {
        public ActionDefence(IPirateGame g, Pirate p, EnemyPredictor ep)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.Defence;
        }

        public override int DoIt(int PM)
        {
            Game.Defend(Player);
            return PM;
        }
    }
    public class ActionGetPowerUp : Actions
    {
        private int PossibleMoves = 0;
        private int NeededMoves;
        private Powerup SelectedPowerUp;

        public ActionGetPowerUp(IPirateGame g, Pirate p, EnemyPredictor ep, Powerup pu)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.GetPowerUp;
            init(pu);
        }

        public void init(Powerup pu)
        {
            SelectedPowerUp = pu;
        }

        public int HowManySteps()
        {
            NeededMoves = Game.Distance(Player, SelectedPowerUp.Location);
            return NeededMoves;
        }

        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            CA.init(true, true, true);
            if (PossibleMoves >= NeededMoves)
            {
                Location l = CA.TryAdd(Game.GetSailOptions(Player, SelectedPowerUp.Location, NeededMoves));
                Game.SetSail(Player, l);
                return NeededMoves;
            }
            else
            {
                Location l = CA.TryAdd(Game.GetSailOptions(Player, SelectedPowerUp.Location, PossibleMoves));
                Game.SetSail(Player, l);
                return PossibleMoves;
            }
        }
    }
    public class ActionRunAway : Actions
    {

        private List<Pirate> EnemiesInRange = new List<Pirate>();
        private List<Location> PossibleLocations = new List<Location>();
        private Location NewLocation;
        private int NeededMoves;
        private int PossibleMoves;

        public ActionRunAway(IPirateGame g, Pirate p, EnemyPredictor ep)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.RunAway;
        }

        public List<Location> AmIInRange()
        {
            foreach (Pirate e in Game.EnemyPiratesWithoutTreasures())
            {
                if (Game.InRange(e, Player) && e.TurnsToSober == 0)
                    EnemiesInRange.Add(e);
            }
            return FindBestLocation();
        }

        private List<Location> FindBestLocation()
        {
            if (EnemiesInRange.Count > 0)
            {
                for (int i = -(Game.GetActionsPerTurn()); i <= Game.GetActionsPerTurn(); i++)
                {
                    for (int j = -(Game.GetActionsPerTurn()); j <= Game.GetActionsPerTurn(); j++)
                    {
                        Location l = new Location(Player.Location.Row + i, Player.Location.Col + j);
                        bool Add = true;
                        foreach (Pirate e in EnemiesInRange)
                        {
                            if (Game.InRange(e, l))
                            {
                                Add = false;
                                break;
                            }
                        }
                        if (Add && CA.CheckLocation(l))
                        {
                            PossibleLocations.Add(l);
                        }
                    }
                }
            }
            return PossibleLocations;
        }

        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            if (PossibleMoves >= NeededMoves)
            {
                CA.init();
                Location l = CA.TryAdd(PossibleLocations);
                Game.SetSail(Player, l);
                return NeededMoves;
            }
            return 0;
        }
    }
    public class ActionSaveHim : Actions
    {
        private Pirate MyDrunkPirate;
        private int PossibleMoves = 0;
        private int NeededMoves;


        public ActionSaveHim(IPirateGame g, Pirate p, EnemyPredictor ep, Pirate mdp)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.SaveHim;
            MyDrunkPirate = mdp;
        }

        public void init(Pirate mdp)
        {
            MyDrunkPirate = mdp;
        }
        public int HowManySteps()
        {
            NeededMoves = Game.Distance(Player, MyDrunkPirate);
            return NeededMoves;
        }
        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            if (PossibleMoves >= NeededMoves)
            {
                CA.init();
                Location l = CA.TryAdd(Game.GetSailOptions(Player, MyDrunkPirate, NeededMoves));
                Game.SetSail(Player, l);
                return NeededMoves;
            }
            else
            {
                CA.init(true, true, true);
                Location l = CA.TryAdd(Game.GetSailOptions(Player, MyDrunkPirate, PossibleMoves));
                Game.SetSail(Player, l);
                return PossibleMoves;
            }
        }

    }
    public class ActionCrash : Actions
    {
        private Pirate Enemy;
        private int PossibleMoves;
        private int NeededMoves;
        private bool CrashNow = true;

        public ActionCrash(IPirateGame g, Pirate p, Pirate e, EnemyPredictor ep)
            : base(g, p, ep)
        {
            ActionType = TypeOfAction.Crash;
            init(e);
        }

        public void init(Pirate e)
        {
            Enemy = e;
        }

        public Pirate ENEMY { get { return Enemy; } set { Enemy = value; } }

        public int Attack()
        {
            Location EnemyLocation = EP.GetEnemyNextLocation(Enemy);
            if (Game.Distance(Player, EnemyLocation) > Game.GetActionsPerTurn())
            {
                CrashNow = false;
            }
            NeededMoves = Game.Distance(Player, EnemyLocation);
            return NeededMoves;
        }

        public override int DoIt(int PM)
        {
            PossibleMoves = PM;
            if (PossibleMoves >= NeededMoves && CrashNow)
            {
                Game.SetSail(Player, EP.GetEnemyNextLocation(Enemy));
                return NeededMoves;
            }
            else
            {
                CA.init(true, true, true);
                Location l = CA.TryAdd(Game.GetSailOptions(Player, Enemy, PossibleMoves));
                Game.SetSail(Player, l);
                return PossibleMoves;
            }
        }
    }


    public class ActionRegularAttack
    {
        public Pirate Attacker;
        public Pirate Target;
        public IPirateGame game;
        public CollisionAvoider CA;
        public int MovesLeft = 0;
        public bool HasShot = false;
        public bool hasmoves = false;
        public bool Chased = false;

        // Find Best Match for atacker - target ;
        public void FindBestMatch(List<Pirate> MyPirates, List<Pirate> EnemyPirates)
        {
            int Min = 1000;


            Pirate MyP = game.MyPirates()[0];
            Pirate EP = game.EnemyPirates()[0];


            foreach (Pirate e in EnemyPirates)
            {
                if (game.Distance(e.InitialLocation, e.Location) < Min)
                {
                    Min = game.Distance(e.InitialLocation, e.Location);
                    EP = e;
                }
            }
            // *********************** add if min < 7 ************************
            Min = 1000;
            foreach (Pirate p in MyPirates)
            {
                if (game.Distance(p, EP.Location) < Min)
                {
                    Min = game.Distance(p, EP.Location);
                    MyP = p;
                }
            }


            //game.Debug("Min Distance from target = {0} ",Min);
            this.Target = EP;
            this.Attacker = MyP;
            //game.Debug("Attacker = {0} , Target = {1}", Attacker.Id, Target.Id);

        }

        // check if the pirate will be able to reach the target 
        private bool CanReach()
        {
            return ((game.Distance(Target.Location, Target.InitialLocation) / (Target.CarryTreasureSpeed)) > (game.Distance(Attacker.Location, Target.Location) / (game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count)));
        }

        public void DoTurn()
        {
            if (CanReach())
            {
                if (Attacker != null && Target != null)
                {
                    game.Debug("Pirate {0} is in attacking Mode, Target = {1}", Attacker.Id, Target.Id);
                }

                int DistanceFromTarget = game.Distance(Attacker, Target);

                int index = 0;

                if (Attacker.ReloadTurns == 0)
                {
                    if (game.InRange(Attacker, Target) == false)
                        index = 0;
                    if (game.InRange(Attacker, Target) && Target.DefenseExpirationTurns == 0 && Target.DefenseReloadTurns > 0)
                        index = 1;
                    if (game.InRange(Attacker, Target) && Target.DefenseExpirationTurns == 0 && Target.DefenseReloadTurns == 0)
                        index = 2;
                    if (game.InRange(Attacker, Target) && Target.DefenseExpirationTurns > 0)
                        index = 3;
                }
                else
                {
                    if (game.InRange(Attacker, Target))
                        index = 3;
                    else
                        index = 0;

                }

                game.Debug(" index ==>{0}", index);

                switch (index)
                {
                    case 0:
                        hasmoves = GoToTarget();
                        break;
                    case 1:
                        hasmoves = ShootTarget();
                        if (hasmoves)
                            return;
                        break;
                    case 2:
                        hasmoves = ChaseTarget();
                        break;
                    case 3:
                        hasmoves = CrashTarget(); // might change to crash, not sure yet
                        break;

                    default:
                        game.Debug("an error was made");
                        break;
                }
            }
            else
            {
                this.MovesLeft = 0;
                hasmoves = false;
            }


        }

        private bool CrashTarget()
        {
            if (MovesLeft == 0)
                MovesLeft = Target.CarryTreasureSpeed;
            Location TargtDest = game.GetSailOptions(Target, Target.InitialLocation, Target.CarryTreasureSpeed)[0];
            Location MyDesy = game.GetSailOptions(Attacker, TargtDest, MovesLeft)[0];
            game.SetSail(Attacker, MyDesy);
            game.Debug("Set To Crash Target at {0}-{1}", MyDesy.Row, MyDesy.Col);
            return true;
        }

        private bool ChaseTarget()
        {
            Location nHome = new Location(Target.InitialLocation.Row - 1, Target.InitialLocation.Col - 1);
            List<Location> n = game.GetSailOptions(Target, nHome, Target.CarryTreasureSpeed * 2);
            Location dest = CA.TryAdd(game.GetSailOptions(Attacker, n[0], game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count));
            if (dest != null)
            {
                if (MovesLeft == 0)
                    MovesLeft = Target.CarryTreasureSpeed;
                game.Debug("Moves = {0}", game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count);
                game.Debug("Location = {0}-{1}, Target at ={2}-{3}", dest.Row, dest.Col, Target.Location.Row, Target.Location.Col);
                game.SetSail(Attacker, dest);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool ShootTarget()
        {
            game.Debug("Pirate {0} Shotting at pirate {1}", Attacker.Id, Target.Id);
            game.Attack(Attacker, Target);
            HasShot = true;
            hasmoves = true;
            return true;


        }

        private bool GoToTarget()
        {
            if (CanReach())
            {

                Location nHome = new Location(Target.InitialLocation.Row - 1, Target.InitialLocation.Col - 1);
                List<Location> n = game.GetSailOptions(Target, nHome, Target.CarryTreasureSpeed * 2);
                Location dest = CA.TryAdd(game.GetSailOptions(Attacker, n[0], game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count));
                if (dest != null)
                {
                    if (MovesLeft == 0)
                        MovesLeft = game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count;
                    game.Debug("Moves = {0}", game.GetActionsPerTurn() - game.MyPiratesWithTreasures().Count);
                    game.Debug("Location = {0}-{1}, Target at ={2}-{3}", dest.Row, dest.Col, Target.Location.Row, Target.Location.Col);
                    game.SetSail(Attacker, dest);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
    public class ActionRegularGetTreasure
    {
        public Pirate Collector;
        Treasure Target;
        public IPirateGame Game;
        public CollisionAvoider CA;
        public bool hasShot = false;
        public int Moves = 0;

        public void DoTurn()
        {
            if (Collector.ReloadTurns == 0)
                hasShot = TestShot();
            if (!hasShot)
            {
                //game.Debug("Pirate {0} in Collector", Collector.Id);
                //game.Debug("C 1");
                Location Dest = CA.TryAdd(Game.GetSailOptions(Collector, Target.Location, Moves));
                if (Dest != null)
                {
                    //game.Debug("C 2");
                    Game.SetSail(Collector, Dest);
                    return;
                }
                Location newTreasure = new Location(Target.Location.Row - 1, Target.Location.Col - 1);
                Dest = CA.TryAdd(Game.GetSailOptions(Collector, newTreasure, Moves));
                if (Dest != null)
                {
                    //game.Debug("C 3");
                    Game.SetSail(Collector, Dest);
                    return;
                }
                newTreasure = new Location(Target.Location.Row + 1, Target.Location.Col + 1);
                Dest = CA.TryAdd(Game.GetSailOptions(Collector, newTreasure, Moves));
                if (Dest != null)
                {
                    //game.Debug("C 4");
                    Game.SetSail(Collector, Dest);
                    return;
                }

            }
        }

        private bool TestShot()
        {
            foreach (Pirate p in Game.EnemySoberPirates())
            {
                if (Game.InRange(Collector.Location, p.Location) && p.DefenseExpirationTurns == 0 && p.DefenseReloadTurns > 0)
                {
                    Game.Attack(Collector, p);
                    return true;
                }
            }
            return false;
        }

        public void Findbest(List<Pirate> Pirates, IPirateGame game)
        {
            int MinTresure = 10000;
            if (Pirates.Count == 0 || game.Treasures().Count == 0)
                return;
            Pirate collector = Pirates[0];
            Treasure treasure = game.Treasures()[0];

            foreach (Pirate p in Pirates)
            {
                foreach (Treasure t in game.Treasures())
                {
                    if (game.Distance(t.Location, p.InitialLocation) < MinTresure)
                    {
                        MinTresure = game.Distance(t.Location, p.InitialLocation);
                        collector = p;
                        treasure = t;
                    }
                }
            }
            this.Collector = collector;
            this.Target = treasure;
            //game.Debug("Collector = {0} Tresure = {1}", Collector.Id, Target.Id);
        }
        public void FindBest(List<Pirate> Pirates)
        {
            Treasure Tresures = Game.Treasures()[0];
            Pirate Collector = Pirates[0];
            double MinScore = CaculateScore(Collector, Tresures);

            foreach (Pirate p in Pirates)
            {
                foreach (Treasure t in Game.Treasures())
                {
                    if (CaculateScore(p, t) < MinScore)
                    {
                        MinScore = CaculateScore(p, t);
                        Collector = p;
                        Tresures = t;
                    }
                }
            }
            this.Collector = Collector;
            this.Target = Tresures;
        }

        private double CaculateScore(Pirate collector, Treasure tresures)
        {
            double a = Game.Distance(collector.Location, tresures.Location) / 6;
            double b = Game.Distance(collector.InitialLocation, tresures.Location);
            double c = tresures.Value;
            return (a + b) / c;
        }
    }
}
