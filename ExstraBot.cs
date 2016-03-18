using Pirates;
using System;
using System.Collections.Generic;

namespace MyBot
{

    public class ExstraBot
    {
        public Pirate p;
        IPirateGame Game;
        private Location Destenation = null;
       
        public int Moves = 0;
        public ExstraBot(Pirate pirate, IPirateGame Game,int Moves)
        {
            this.p = pirate;
            this.Game = Game;
            this.Moves = Moves;
        }

        public List<Location> ExstraBotPosiibleLocation(int NumberOfMoves)
        {
            //הסתכלות על היריב
            //בדיקה האם אפשר להגיע ליריב
            //תקיפה ואז החלפה
            //או חיפוש אוצרות הכי קרובים
            if (!p.HasTreasure)
            {
                List<Pirate> EnemyPirateWithTresures = Game.EnemyPiratesWithTreasures();
                if (!(EnemyPirateWithTresures.Count == 0))
                {
                    //חישוב צעדים של יריב מהבית
                    //חישוב צעדים שלי ממרחק בניים
                    Pirate ToCheck = FindClosestPirate(p, EnemyPirateWithTresures);
                    bool Answer = CheckIfPossible(ToCheck, NumberOfMoves);
                    if (Answer)
                    {
                        Destenation = new Location(ToCheck.InitialLocation.Row, ToCheck.InitialLocation.Col);
                        return SetSailToBot(Destenation, NumberOfMoves);
                    }
                    else
                    {
                        return RegularMove(NumberOfMoves);
                    }
                }
                else
                {
                    return RegularMove(NumberOfMoves);
                }

            }
            return SetSailToBot(p.InitialLocation, NumberOfMoves);



        }

        private List<Location> RegularMove(int NumberOfMoves)
        {
           //Add  - Find Tresure;
            return null;
        }
       

        private bool CheckIfPossible(Pirate ToCheck, int NumberOfMoves)
        {
            return Game.Distance(ToCheck.Location, ToCheck.InitialLocation) > (Game.Distance(p.Location, new Location(ToCheck.InitialLocation.Row, ToCheck.InitialLocation.Col)) / NumberOfMoves);
        }

        private List<Location> SetSailToBot(Location Destenation, int NumberOfMoves)
        {
            return Game.GetSailOptions(p, Destenation, NumberOfMoves);
        }

        public Pirate FindClosestPirate(Pirate MyPirate, List<Pirate> EnemyPirates)
        {
            Pirate p = EnemyPirates[0];
            foreach (Pirate pi in EnemyPirates)
            {
                if (Game.Distance(pi.Location, MyPirate.Location) < Game.Distance(p.Location, MyPirate.Location))
                {
                    p = pi;
                }
            }
            return p;
        }
    }


}

