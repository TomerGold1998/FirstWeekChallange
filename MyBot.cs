using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        bool EscortMe = false;

        public void DoTurn(IPirateGame game)
        {
            if (game.AllMyPirates().Count() == 2 && game.Treasures().Count() == 1)
            {
                EscortMe = true;
            }
            if (EscortMe)
            {
           
                List<Pirate> MY = game.MyPirates();
                List<Pirate> Enemy = game.EnemyPirates();
                List<Treasure> T = game.Treasures();
                if (MY[0].HasTreasure)
                {
                    if (MY[1].TurnsToSober == 0)
                    {
                        List<Location> Des1 = game.GetSailOptions(MY[1], Enemy[1], 5);
                        int a = Des1.Count();
                        game.SetSail(MY[1], Des1[a-1]);
                    }
                    List<Location> home = game.GetSailOptions(MY[0], MY[0].InitialLocation, 1);
                    game.SetSail(MY[0], home[0]);
                }
                if (Enemy[0].HasTreasure == false && Enemy[1].HasTreasure == false)
                {
                    if (Enemy[1].TurnsToSober > 0)
                    {
                        List<Location> Des1 = game.GetSailOptions(MY[0], T[0], 6);
                        game.SetSail(MY[0], Des1[0]);
                    }
                    else
                    {
                        List<Location> Des1 = game.GetSailOptions(MY[1], Enemy[1], 3);
                        List<Location> Des2 = game.GetSailOptions(MY[0], T[0], 3);
                        game.SetSail(MY[1], Des1[0]);
                        game.SetSail(MY[0], Des2[0]);
                    }
                }
                else
                {
                    game.Debug("1 ");
                    if (game.InRange(MY[1], Enemy[1]) && (Enemy[1].TurnsToSober == 0))
                    {
                        game.Attack(MY[1], Enemy[1]);
                        game.Debug("2 ");
                        return;
                    }
                    else
                    {
                        if ((Enemy[1].TurnsToSober > 0) && (Enemy[0].HasTreasure))
                        {
                            if (game.InRange(MY[0], Enemy[0]))
                            {
                                game.Debug("3 ");
                                game.Attack(MY[0], Enemy[0]);
                                return;
                            }
                            game.Debug("4 ");
                            List<Location> Des2 = game.GetSailOptions(MY[0], Enemy[0], 6);
                            game.SetSail(MY[0], Des2[0]);
                            return;
                        }
                        Location Des1 = new Location(MY[1].Location.Row + 1, MY[1].Location.Col);
                        game.SetSail(MY[1], Des1);
                        game.Debug("HERE :{0} - {1} ",Des1.Row,Des1.Col);
                        return;
                    }
                                                
                }

            }
        }
    }
}
