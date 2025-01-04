using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    public class Game
    {
        /// <summary>
        /// All Milo games.
        /// </summary>
        public enum MiloGame
        {
            FreQuency,
            Amplitude,
            KaraokeRevolution,
            KaraokeRevolutionVolume2,
            KaraokeRevolutionVolume3,
            EyeToyAntiGrav,
            GuitarHero,
            GuitarHero2_PS2,
            GuitarHero2_360,
            GuitarHeroEncoreRocksThe80s,
            Phase,
            RockBand,
            RockBand2,
            TheBeatlesRockBand,
            LegoRockBand,
            GreenDayRockBand,
            RockBand3Bank5, // prototype, only including because I want to support converting assets from this
            RockBand3Bank6, // prototype, only including because I want to support converting assets from this
            RockBand3,
            DanceCentral,
            DanceCentral2,
            RockBandBlitz,
            DanceCentral3,
        }

        /// <summary>
        /// Gets a name for the given game.
        /// </summary>
        /// <param name="game">The game (you lost btw).</param>
        /// <returns>The name of the game.</returns>
        public static string GetName(MiloGame game)
        {
            switch (game)
            {
                case MiloGame.FreQuency:
                    return "FreQuency";
                case MiloGame.Amplitude:
                    return "Amplitude";
                case MiloGame.KaraokeRevolution:
                    return "Karaoke Revolution";
                case MiloGame.KaraokeRevolutionVolume2:
                    return "Karaoke Revolution Volume 2";
                case MiloGame.KaraokeRevolutionVolume3:
                    return "Karaoke Revolution Volume 3";
                case MiloGame.EyeToyAntiGrav:
                    return "EyeToy: AntiGrav";
                case MiloGame.GuitarHero:
                    return "Guitar Hero";
                case MiloGame.GuitarHero2_360:
                    return "Guitar Hero II";
                case MiloGame.GuitarHeroEncoreRocksThe80s:
                    return "Guitar Hero Encore: Rocks the 80s";
                case MiloGame.Phase:
                    return "Phase";
                case MiloGame.RockBand:
                    return "Rock Band";
                case MiloGame.RockBand2:
                    return "Rock Band 2";
                case MiloGame.TheBeatlesRockBand:
                    return "The Beatles: Rock Band";
                case MiloGame.LegoRockBand:
                    return "Lego Rock Band";
                case MiloGame.GreenDayRockBand:
                    return "Green Day: Rock Band";
                case MiloGame.RockBand3Bank5:
                    return "Rock Band 3 Bank5";
                case MiloGame.RockBand3Bank6:
                    return "Rock Band 3 Bank6";
                case MiloGame.RockBand3:
                    return "Rock Band 3";
                case MiloGame.DanceCentral:
                    return "Dance Central";
                case MiloGame.DanceCentral2:
                    return "Dance Central 2";
                case MiloGame.RockBandBlitz:
                    return "Rock Band Blitz";
                case MiloGame.DanceCentral3:
                    return "Dance Central 3";
                default:
                    return "Unknown";
            }
        }

        public static uint GetRevisionForGame(object obj, MiloGame game)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Input object cannot be null.");
            }

            var fieldInfo = obj.GetType().GetField("gameRevisions");
            if (fieldInfo == null)
            {
                throw new InvalidOperationException($"Game revisions not found in type {obj.GetType().Name}.");
            }

            var gameRevisions = fieldInfo.GetValue(obj) as Dictionary<MiloGame, uint>;
            if (gameRevisions == null)
            {
                throw new InvalidOperationException($"Game revisions in type {obj.GetType().Name} is null or not of the expected type.");
            }

            if (!gameRevisions.ContainsKey(game))
            {
                throw new KeyNotFoundException($"Game not found in game revisions dictionary for type {obj.GetType().Name}.");
            }

            return gameRevisions[game];
        }
    }
}
