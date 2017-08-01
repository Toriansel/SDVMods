﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TwilightCore.PRNG;
using TwilightCore.StardewValley;
using StardewValley;

namespace ClimateOfFerngill
{
    public class FerngillFog
    {
        private Rectangle FogSource = new Microsoft.Xna.Framework.Rectangle(640, 0, 64, 64);
        private Color FogColor { get; set; }
        private float FogAlpha { get; set; }
        public SDVTime FogExpirTime { get; set; }
        public bool FogTypeDark { get; set; }
        public bool AmbientFog { get; set; }
        private Vector2 FogPosition { get; set; }

        public FerngillFog()
        {
            FogExpirTime = new SDVTime(600);
        }

        public FerngillFog(Color FogColor, float FogAlpha, SDVTime FogExpirTime, bool FogTypeDark, 
            bool AmbientFog, Vector2 FogPosition)
        {
            this.FogColor = FogColor;
            this.FogAlpha = FogAlpha;
            this.FogExpirTime = FogExpirTime;
            this.FogTypeDark = FogTypeDark;
            this.AmbientFog = AmbientFog;
            this.FogPosition = FogPosition;
        }

        public void Reset()
        {
            FogTypeDark = false;
            AmbientFog = false;
            FogAlpha = 0f;
            FogExpirTime = new SDVTime(0600);
        }

        public void IsDarkFog()
        {
            FogTypeDark = true;
        }

        public bool IsFogVisible()
        {
            if (FogAlpha > 0 || AmbientFog)
                return true;
            else
                return false;
        }

        public bool IsFog(MersenneTwister Dice, FerngillWeather CurrentWeather, string season)
        {
            //set up fog.
            double FogChance = CurrentWeather.GetFogOdds(SDVDate.Today);
            
            //move these out of the main loop.
            if (CurrentWeather.CurrentConditions() == SDVWeather.Rainy || CurrentWeather.CurrentConditions() == SDVWeather.Debris)
                return false;

            if (Dice.NextDouble() < FogChance)
            {
                return true;
            }
            else
                return false;
        }

        public void CheckForFog(MersenneTwister Dice, FerngillWeather CurrentWeather)
        {
            if (IsFog(Dice, CurrentWeather, Game1.currentSeason))
            {
                CreateFog(FogAlpha: .55f, AmbientFog: true, FogColor: (Color.White * 1.35f));
                Game1.globalOutdoorLighting = .5f;

                if (Dice.NextDouble() < .15)
                {
                    IsDarkFog();
                    Game1.outdoorLight = new Color(227, 222, 211);
                }
                else
                {
                    Game1.outdoorLight = new Color(179, 176, 171);
                }

                double FogTimer = Dice.NextDouble();
                SDVTime FogExpirTime = new SDVTime(1200);

                if (FogTimer > .75 && FogTimer <= .90)
                {
                    FogExpirTime = new SDVTime(1120);
                }
                else if (FogTimer > .55 && FogTimer <= .75)
                {
                    FogExpirTime = new SDVTime(1030);
                }
                else if (FogTimer > .30 && FogTimer <= .55)
                {
                    FogExpirTime = new SDVTime(930);
                }
                else if (FogTimer <= .30)
                {
                    FogExpirTime = new SDVTime(820);
                }

                this.FogExpirTime = FogExpirTime;
            }
        }

        public void UpdateFog(int time)
        {

            if (FogTypeDark)
            {
                if (time == (FogExpirTime - 30).ReturnIntTime())
                {
                    Game1.globalOutdoorLighting = .98f;
                    Game1.outdoorLight = new Color(200, 198, 196);
                }

                if (time == (FogExpirTime - 20).ReturnIntTime())
                {
                    Game1.globalOutdoorLighting = .99f;
                    Game1.outdoorLight = new Color(179, 176, 171);
                }

                if (time == (FogExpirTime - 10).ReturnIntTime())
                {
                    Game1.globalOutdoorLighting = 1f;
                    Game1.outdoorLight = new Color(110, 109, 107);
                }
            }
            else
            {
                if (time == (FogExpirTime - 30).ReturnIntTime())
                {
                    Game1.globalOutdoorLighting = .80f;
                    Game1.outdoorLight = new Color(168, 142, 99);
                }

                if (time == (FogExpirTime - 20).ReturnIntTime())
                {
                    Game1.globalOutdoorLighting = .92f;
                    Game1.outdoorLight = new Color(117, 142, 99);

                }

                if (time == (FogExpirTime - 10).ReturnIntTime())
                {
                    Game1.globalOutdoorLighting = .96f;
                    Game1.outdoorLight = new Color(110, 109, 107);
                }
            }

            //it helps if you implement the fog cutoff!
            if (time == FogExpirTime.ReturnIntTime())
            {
                this.AmbientFog = false;
                Game1.globalOutdoorLighting = 1f;
                Game1.outdoorLight = Color.White;
                FogAlpha = 0f; //fixes it lingering.
            }

        }

        public void DrawFog()
        {
            if (IsFogVisible())
            {
                Vector2 position = new Vector2();
                float num1 = -64 * Game1.pixelZoom + (int)(FogPosition.X % (double)(64 * Game1.pixelZoom));
                while (num1 < (double)Game1.graphics.GraphicsDevice.Viewport.Width)
                {
                    float num2 = -64 * Game1.pixelZoom + (int)(FogPosition.Y % (double)(64 * Game1.pixelZoom));
                    while ((double)num2 < Game1.graphics.GraphicsDevice.Viewport.Height)
                    {
                        position.X = (int)num1;
                        position.Y = (int)num2;
                        Game1.spriteBatch.Draw(Game1.mouseCursors, position, new Microsoft.Xna.Framework.Rectangle?
                            (FogSource), FogAlpha > 0.0 ? FogColor * FogAlpha : Color.Black * 0.95f, 0.0f, Vector2.Zero, Game1.pixelZoom + 1f / 1000f, SpriteEffects.None, 1f);
                        num2 += 64 * Game1.pixelZoom;
                    }
                    num1 += 64 * Game1.pixelZoom;
                }
            }
        }

        public void MoveFog()
        {
            if (AmbientFog)
            {
                this.FogPosition = Game1.updateFloatingObjectPositionForMovement(FogPosition,
                    new Vector2(Game1.viewport.X, Game1.viewport.Y), Game1.previousViewportPosition, -1f);
                FogPosition = new Vector2((FogPosition.X + 0.5f) % (64 * Game1.pixelZoom), 
                    (FogPosition.Y + 0.5f) % (64 * Game1.pixelZoom));
            }
        }

        public void CreateFog(float FogAlpha, bool AmbientFog, Color FogColor)
        {
            this.FogAlpha = FogAlpha;
            this.AmbientFog = AmbientFog;
            this.FogColor = FogColor;
        }
    }
}