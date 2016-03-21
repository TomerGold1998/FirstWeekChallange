using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace MyBot
{
    class CollitionAvoider
    {
        private List<Location> CaptureLocation = new List<Location>();

        public IPirateGame Game;

        public void Reset()
        {
            this.CaptureLocation.Clear();
        }
        public void Init(bool withTresuars)
        {
            foreach (Pirate p in Game.MyDrunkPirates())
            {
                CaptureLocation.Add(p.Location);
            }
            foreach (Pirate p in Game.EnemyDrunkPirates())
            {
                CaptureLocation.Add(p.Location);
            }
            foreach (Pirate p in Game.EnemyPiratesWithTreasures())
            {
                List<Location> ETW = Game.GetSailOptions(p, p.InitialLocation, 1);
                foreach (Location l in ETW)
                {
                    CaptureLocation.Add(l);
                }
            }
            foreach (Pirate p in Game.MyPirates())
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
    public class MyBot : Pirates.IPirateBot
    {
        bool work2gether = false;
        CollitionAvoider CA = new CollitionAvoider();

        public void DoTurn(IPirateGame game)
        {
            CA.Game = game;
            CA.Reset();



            List<Pirate> My = game.MyPirates();
            List<Pirate> Enemy = game.EnemyPirates();
            List<Pirate> MyWoT = game.MyPiratesWithoutTreasures();
            List<Location> Des;
            Location Destination;
            if ((My.Count() == 2) && (game.Treasures().Count() > 1))
                work2gether = true;
            if (work2gether)
            {
                if (My.Count == 2 && Enemy.Count == 2)
                {
                    game.Debug("1");
                    if (!checkift(Enemy[0], Enemy[1]) || (checkift(My[0], My[1])))
                    {
                        game.Debug("2");
                        if (checkift(My[0], My[1]))
                        {
                            CA.Init(false);
                            game.Debug("3");
                            for (int i = 0; i < My.Count(); i++)
                            {
                                Des = game.GetSailOptions(My[i], My[i].InitialLocation, 1);
                                Destination = CA.TryAdd(Des);
                                if (Destination != null)
                                    game.SetSail(My[i], Destination);
                            }
                            return;
                        }
                        if (Enemy[0].TurnsToSober > 0 && Enemy[1].TurnsToSober > 0)
                        {
                            CA.Init(false);
                            game.Debug("3.1");
                            int steps = 6;
                            for (int i = 0; i < My.Count(); i++)
                                if (My[i].HasTreasure)
                                {
                                    game.Debug("3.2");
                                    Des = game.GetSailOptions(My[i], My[i].InitialLocation, 1);
                                    Destination = CA.TryAdd(Des);
                                    if (Destination != null)
                                        game.SetSail(My[i], Destination);
                                    steps -= 1;
                                }
                            for (int i = 0; i < My.Count(); i++)
                                if (!My[i].HasTreasure)
                                {
                                    game.Debug("3.3");
                                    Des = game.GetSailOptions(My[i], closestT(game, My[i]), steps);
                                    Destination = CA.TryAdd(Des);
                                    if (Destination != null)
                                        game.SetSail(My[i], Destination);
                                    return;
                                }
                            return;
                        }
                        if (SAndT(game) != null)
                        {
                            CA.Init(true);
                            game.Debug("4");
                            if (My[0].ReloadTurns > 0 && !My[1].HasTreasure)
                            {
                                game.Debug("4.1");
                                if (game.Distance(My[1], SAndT(game)) < 4)
                                {
                                    game.Debug("4.1.1");
                                    game.Attack(My[1], SAndT(game));
                                    return;
                                }
                                Des = game.GetSailOptions(My[0], closestT(game, My[0]), 1);
                                Destination = CA.TryAdd(Des);
                                if (Destination != null)
                                    game.SetSail(My[0], Destination);
                                Des = game.GetSailOptions(My[1], SAndT(game), 5);
                                Destination = CA.TryAdd(Des);
                                if (Destination != null)
                                    game.SetSail(My[1], Destination);
                                return;
                            }
                            if (My[1].ReloadTurns > 0 && !My[0].HasTreasure)
                            {
                                if (game.Distance(My[0], SAndT(game)) < 4)
                                {
                                    game.Attack(My[0], SAndT(game));
                                    return;
                                }
                                Des = game.GetSailOptions(My[1], closestT(game, My[1]), 1);
                                Destination = CA.TryAdd(Des);
                                if (Destination != null)
                                    game.SetSail(My[1], Destination);
                                Des = game.GetSailOptions(My[0], SAndT(game), 5);
                                Destination = CA.TryAdd(Des);
                                if (Destination != null)
                                    game.SetSail(My[0], Destination);
                                return;
                            }
                            /*if (My[0].HasTreasure)
                            {
                                if(game.InRange(My[1], SAndT(game)))
                                {
                                    game.Attack(My[1], SAndT(game));
                                    return;
                                }
                                Des = game.GetSailOptions(My[0],My[0].InitialLocation, 1);
                                Destination = CA.TryAdd(Des);
                                game.SetSail(My[0], Destination);
                                Des = game.GetSailOptions(My[1], SAndT(game), 5);
                                Destination = CA.TryAdd(Des);
                                game.SetSail(My[1], Destination);
                            }
                            if(My[1].HasTreasure)
                            {
                                if (game.InRange(My[0], SAndT(game)))
                                {
                                    game.Attack(My[0], SAndT(game));
                                    return;
                                }
                                Des = game.GetSailOptions(My[1], My[1].InitialLocation, 1);
                                Destination = CA.TryAdd(Des);
                                game.SetSail(My[1], Destination);
                                Des = game.GetSailOptions(My[0], SAndT(game), 5);
                                Destination = CA.TryAdd(Des);
                                game.SetSail(My[0], Destination);
                            }*/
                        }
                        CA.Init(false);
                        game.Debug("5");
                        for (int i = 0; i < My.Count(); i++)
                        {
                            if (My[i].HasTreasure)
                            {
                                Des = game.GetSailOptions(My[i], My[i].InitialLocation, 1);
                                Destination = CA.TryAdd(Des);
                                if (Destination != null)
                                    game.SetSail(My[i], Destination);
                            }
                            else
                            {
                                Location L = new Location(My[i].InitialLocation.Row - 1, closestT(game, My[i]).Location.Col);
                                Des = game.GetSailOptions(My[i], L, 2);
                                Destination = CA.TryAdd(Des);
                                if (Destination != null)
                                    game.SetSail(My[i], Destination);
                            }
                        }
                        return;
                    }
                    else
                    {
                        CA.Init(true);
                        game.Debug("6 :" + game.GetAttackRadius());

                        if (game.Distance(MyWoT[0], closestP(game)) < 4)
                        {
                            game.Debug("7");
                            game.Attack(MyWoT[0], closestP(game));
                            return;
                        }
                        game.Debug("8");
                        Des = game.GetSailOptions(MyWoT[0], closestP(game), 6);
                        Destination = CA.TryAdd(Des);
                        if (Destination != null)
                            game.SetSail(MyWoT[0], Destination);
                        return;
                    }
                }
            }
        }
        public bool checkift(Pirate p1, Pirate p2)
        {
            if (p1.HasTreasure && p2.HasTreasure)
                return true;
            return false;
        }
        public Treasure closestT(IPirateGame game, Pirate P)
        {
            int min = 1000;
            Treasure T = game.Treasures()[0];
            for (int i = 0; i < game.Treasures().Count(); i++)
                if (game.Distance(P.InitialLocation, game.Treasures()[i].Location) < min)
                {
                    min = game.Distance(P.InitialLocation, game.Treasures()[i].Location);
                    T = game.Treasures()[i];
                }
            return T;
        }
        public Pirate closestP(IPirateGame game)
        {
            int min = 1000;
            List<Pirate> AP = game.EnemyPirates();
            Pirate P = AP[0];
            for (int i = 0; i < AP.Count(); i++)
                if (game.Distance(AP[i], AP[i].InitialLocation) < min)
                {
                    min = game.Distance(AP[i], AP[i].InitialLocation);
                    P = game.EnemyPirates()[i];
                }
            return P;
        }
        public Pirate SAndT(IPirateGame game)
        {
            if (game.AllEnemyPirates()[0].HasTreasure && game.AllEnemyPirates()[1].TurnsToSober > 0)
                return game.AllEnemyPirates()[0];
            if (game.AllEnemyPirates()[1].HasTreasure && game.AllEnemyPirates()[0].TurnsToSober > 0)
                return game.AllEnemyPirates()[1];
            return null;
        }
    }
}