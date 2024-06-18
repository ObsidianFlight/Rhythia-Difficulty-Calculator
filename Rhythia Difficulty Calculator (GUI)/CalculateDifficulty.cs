using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhythia_Difficulty_Calculator__GUI_
{
    internal class CalculateDifficulty
    {
		public static double Clamp(double value, double min, double max)
		{

			if (value < min)
			{
				return min;
			}
			else if (value > max)
			{
				return max;
			}

			return value;
		}

		public static MapInfo ConvertMap(string mapdata, double speed, bool hasName, string name)
		{
			double x1 = 0;
			double x2 = 0;
			double x3 = 0;
			double y1 = 0;
			double y2 = 0;
			double y3 = 0;
			int clockwise = 0;

			//Splits mapdata into an array so I can use it's information
			string[] mapraw = mapdata.Split(',');
			string mapName = mapraw[0];
			if(hasName == true)
            {
				mapName = name;
            }
			string[] notesraw = new string[mapraw.Length - 1];
			double[,] notearray = new double[notesraw.Length, 3];
			double[,] noteDifficultyArray = new double[notesraw.Length, 10];
			for (int i = 0; i < notesraw.Length; i++)
			{
				notesraw[i] = mapraw[i + 1];
				string[] data = notesraw[i].Split('|');
				notearray[i, 0] = Convert.ToDouble(data[0]);
				notearray[i, 1] = Convert.ToDouble(data[1]);
				notearray[i, 2] = Convert.ToDouble(data[2]) / speed;
			}
			int stack = 0;
			//Grabs all the information I need from the notes, Most likely done in an unefficient manner.
			for (int i = 0; i < notearray.GetLength(0); i++)
			{
				double timeSinceLastNote = 0;
				double distance = 0;
				double distanceFromCenter = 2;
				x1 = notearray[i, 0];
                y1 = notearray[i, 1];
                if (i != 0)
				{
					x2 = notearray[i - 1, 0];
					y2 = notearray[i - 1, 1];
					timeSinceLastNote = notearray[i, 2] - notearray[i - 1, 2];
				}
				double x = Math.Abs(x1 - x2);
				double y = Math.Abs(y1 - y2);
				double cx = Math.Abs(x1 - 1);
				double cy = Math.Abs(y1 - 1);
				distance = Math.Sqrt(x * x + y * y);
				distanceFromCenter = Math.Sqrt(cx * cx + cy * cy);
				double rotation = 0;
				double answer = 0;
				double direction = 12300;
				if (i > 1)
				{
					//"Just use the dot product of (blah blah)"
					//I KNOW!! I don't know how. I hope you like if statements.
					//Finds whether the last 3 notes are clockwise or not.
					x1 = notearray[i - 2 - stack, 0];
					x2 = notearray[i - 1, 0];
					x3 = notearray[i, 0];
					y1 = notearray[i - 2 - stack, 1];
					y2 = notearray[i - 1, 1];
					y3 = notearray[i, 1];
					double a1 = x1 - x2;
					double a2 = y1 - y2;
					double b1 = x3 - x2;
					double b2 = y3 - y2;
					double c1 = x2 - x3;
					double c2 = y3 - y2;
					if (c1 == 0 & c2 > 0) { direction = 90; }
					if (c1 == 0 & c2 < 0) { direction = 270; }
					if (c1 > 0 & c2 == 0) { direction = 0; }
					if (c1 < 0 & c2 == 0) { direction = 180; }
					if (c1 > 0 & c2 > 0)
					{
						//0-90 Quadrant
						direction = Math.Round(Math.Abs(Math.Atan((c2 / c1)) * (180 / Math.PI)), 4);
					}
					if (c1 < 0 & c2 > 0)
					{
						//90-180 Quadrant
						direction = Math.Round(Math.Abs(Math.Atan((c2 / c1)) * (180 / Math.PI)), 4) + 90;
					}
					if (c1 < 0 & c2 < 0)
					{
						//180-270 Quadrant
						direction = Math.Round(Math.Abs(Math.Atan((c2 / c1)) * (180 / Math.PI)), 4) + 180;
					}
					if (c1 > 0 & c2 < 0)
					{
						//270-360 Quadrant
						direction = Math.Round(Math.Abs(Math.Atan((c2 / c1)) * (180 / Math.PI)), 4) + 270;
					}

					//Checks if notes are a stack
					if (Math.Abs(a1) + Math.Abs(a2) != 0 & Math.Abs(b1) + Math.Abs(b2) != 0)
					{
						rotation = Math.Acos((((a1 * b1) + (a2 * b2)) / (Math.Sqrt((a1 * a1 + a2 * a2)) * Math.Sqrt((b1 * b1 + b2 * b2)))));
						stack = 0;
					}
					else if (distance == 0)
					{
						rotation = 180 * (Math.PI / 180);
						stack += 1;
					}
					else
					{
						//This shouldn't happen, I think. Sometimes it does though.
						//Console.WriteLine("else!");
						stack = 0;
						rotation = 180 * (Math.PI / 180);
					}
					answer = Math.Round(rotation * (180 / Math.PI), 4);
				}
				if (i == 0 || i == 1) { answer = 180; }
				noteDifficultyArray[i, 0] = notearray[i, 2]; //Time in song in milliseconds
				noteDifficultyArray[i, 1] = timeSinceLastNote; //in milliseconds
				noteDifficultyArray[i, 2] = distance;
				noteDifficultyArray[i, 3] = stack; //the amount of notes stacked behind it
				noteDifficultyArray[i, 4] = direction; //only used for rotation/flow detection, not that I put much thought into it.
				noteDifficultyArray[i, 5] = answer; //angle between the current note and the last 2.
				noteDifficultyArray[i, 8] = distanceFromCenter;
				noteDifficultyArray[i, 9] = 0; //Changes further down in code to detect maps doing BS quantum offgrids.

				if (i > 0)
				{
					double n = noteDifficultyArray[i - 1, 4];
					if (n > 180) { n -= 360; }
					double m = noteDifficultyArray[i, 4];
					if (m > 180) { m -= 360; }
					double k = 0;
					if (n > 0) { k = m - n; }
					if (n < 0) { k = m - n; }
					if (n == 0) { k = m; }
					if (k < 0) { k += 360; }
					if (k > 0 & k < 180) { clockwise = -1; }
					if (k == 0 || k == 180) { clockwise = 0; }
					if (k > 180) { clockwise = 1; }
				}
				noteDifficultyArray[i, 6] = clockwise; // 0 = straight, 1 = clockwise, -1 = counterclockwise, 
			}

			int[] flowChecker = new int[noteDifficultyArray.GetLength(0)];
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				if (i == 0 || i == 1) { flowChecker[i] = 1; }
				else
				{
					int u = (int)noteDifficultyArray[i - 2, 6];
					int h = (int)noteDifficultyArray[i - 1, 6];
					int b = (int)noteDifficultyArray[i, 6];

					//Most likely when you are spinning
					if (u == 1 & h == 1 & b == 1) { flowChecker[i] = 0; }
					else if (u == -1 & h == -1 & b == -1) { flowChecker[i] = 0; }
					//Quantum Slider, or repetitive back and forth jumps/slides
					//If Angle is 0, Uses 1 instead.
					else if (u == 0 & h == 0 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 1; } else { flowChecker[i] = 0; } }
					//Coming out of repetitive back and forth jumps, or a quantum slider
					else if (u == 0 & h == 0 & b == -1) { flowChecker[i] = 2; }
					else if (u == 0 & h == 0 & b == 1) { flowChecker[i] = 2; }
					//The Wiggle Patterns.
					else if (u == 1 & h == -1 & b == 1) { flowChecker[i] = 2; }
					else if (u == -1 & h == 1 & b == -1) { flowChecker[i] = 2; }
					//Most likely when flow is reversed, or the start of a slider, if flow is hard reversed the angle should be 0.
					else if (u == -1 & h == -1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 1 & h == 1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 1 & h == -1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 2; } }
					else if (u == -1 & h == 1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 2; } }
					//Also reverses flow, I'm not sure how much emphasis I should be putting on these.
					else if (u == 1 & h == 1 & b == -1) { flowChecker[i] = 4; }
					else if (u == -1 & h == -1 & b == 1) { flowChecker[i] = 4; }
					else if (u == 1 & h == -1 & b == -1) { flowChecker[i] = 3; }
					else if (u == -1 & h == 1 & b == 1) { flowChecker[i] = 3; }
					//Most likely found in patterns that include Slides
					else if (u == 1 & h == 0 & b == -1) { flowChecker[i] = 2; }
					else if (u == -1 & h == 0 & b == 1) { flowChecker[i] = 2; }
					//Most likely in Spin Patterns, if not detected in a spin, increase by 3
					else if (u == 1 & h == 0 & u == 1)
					{
						if (noteDifficultyArray[i, 5] < 60)
						{
							flowChecker[i] = 4;
						}
						else
						{
							flowChecker[i] = 1;
						}
					}
					else if (u == -1 & h == 0 & u == 1) { if (noteDifficultyArray[i, 5] < 60) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 0 & h == 1 & b == 0) { if (noteDifficultyArray[i, 5] < 60) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 0 & h == -1 & b == 0) { if (noteDifficultyArray[i, 5] < 60) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else { flowChecker[i] = 1; }
				}
			}

			double prevFinal = 0;
			double consistencyTimer = 0;
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				if (i > 0)
				{
					double final = -1;
					if (noteDifficultyArray[i, 1] > consistencyTimer)
					{
						consistencyTimer += CalculateDifficulty.Clamp(noteDifficultyArray[i, 1] / 2, 1, 1000);
					}
					else
					{
						consistencyTimer -= noteDifficultyArray[i, 1];
						consistencyTimer = CalculateDifficulty.Clamp(consistencyTimer, 0, 5000);
						noteDifficultyArray[i, 1] += consistencyTimer;
					}
					double distanceoverN = 0;
					if (i > 4)
					{
						distanceoverN += noteDifficultyArray[i, 2] + noteDifficultyArray[i - 1, 2] + noteDifficultyArray[i - 2, 2] + noteDifficultyArray[i - 3, 2] + noteDifficultyArray[i - 4, 2];
					}
					final = Math.Pow(Math.Pow(CalculateDifficulty.Clamp(noteDifficultyArray[i, 1] / 1000, 0.02, 9999), -.5), 3.5) / 3.5; // Difficulty Based on Time
					final *= (Math.Pow((noteDifficultyArray[i, 2]), 1.5) / 3.4); // Difficulty Based on Distance
					if (noteDifficultyArray[i, 2] < 1.1) { final *= 1.2; }
					else if (noteDifficultyArray[i, 2] < 1.8 && noteDifficultyArray[i, 5] > 70) { final *= 0.8; }
					else if (noteDifficultyArray[i, 2] > 1.9) { final *= 1.1; }
					if (noteDifficultyArray[i, 2] > 2.5) { final *= 0.8; }
					if (distanceoverN < 3 || noteDifficultyArray[i, 3] > 0) { final *= 0.85; }
					else
					{
						double flow = flowChecker[i] * 0.5;
						if (noteDifficultyArray[i, 2] > 1.2 && noteDifficultyArray[i, 2] < 2 && distanceoverN > 7) { flow *= 1.5; }
						final *= 0.5 + flow;
					} // Difficulty Based on Flow 

					if (noteDifficultyArray[i, 3] > 0) { final = prevFinal * CalculateDifficulty.Clamp(Math.Log(noteDifficultyArray[i, 3]), 0, 1); } //Difficulty Based on Stack
					final *= 1.5 - (Math.Abs(noteDifficultyArray[i, 5] - 45) * 0.5 / 180 * CalculateDifficulty.Clamp(Math.Abs(noteDifficultyArray[i, 2] - 3), 1, 3)); //Difficulty based on angle
					final *= CalculateDifficulty.Clamp(1 + (1 - noteDifficultyArray[i, 8]), 1, 1.5);
					final *= CalculateDifficulty.Clamp(2.4 - noteDifficultyArray[i, 9], 1, 2);
					final += 0.5;
					if (Double.IsNaN(final) == true) { final = 1; }
					noteDifficultyArray[i, 7] = final;
					prevFinal = final;
				}
				//Console.WriteLine(noteDifficultyArray[i, 7]);
			}
			double overallDifficulty = 0;
			double totalSongTime = noteDifficultyArray[noteDifficultyArray.GetLength(0) - 1, 0];
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				overallDifficulty += noteDifficultyArray[i, 7];
				if (i > 0)
				{
					if (noteDifficultyArray[i, 1] > 2500)
					{
						totalSongTime -= (noteDifficultyArray[i, 1] - 100);
					}
				}
			}
			int notesBehind = 0;
			double maxDifficulty = 0;
			double prevTempDiff = 0;
			double averageDifficulty = 0;
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				if (notesBehind < 28) { notesBehind++; }
				double[,] tempDiff = new double[notesBehind, 2];
				for (int j = 0; j < tempDiff.GetLength(0); j++)
				{
					tempDiff[j, 0] = noteDifficultyArray[i - j, 0]; //Gets Time in Song
					tempDiff[j, 1] = noteDifficultyArray[i - j, 7]; //Gets Difficulty
					if (tempDiff[j, 0] < noteDifficultyArray[i, 0] - 2500)
					{
						notesBehind--;
					}
				}
				double tempMaxDifficulty = 0;
				for (int j = 0; j < tempDiff.GetLength(0); j++)
				{
					tempMaxDifficulty += tempDiff[j, 1];
				}
				tempMaxDifficulty /= 4.2;
				if (tempMaxDifficulty > prevTempDiff)
				{
					tempMaxDifficulty -= Math.Log(tempMaxDifficulty - prevTempDiff);
				}
				if (tempMaxDifficulty > maxDifficulty)
				{
					maxDifficulty = tempMaxDifficulty;
				}
				//Console.WriteLine(maxDifficulty + ",  " + tempMaxDifficulty);
				prevTempDiff = tempMaxDifficulty;
				averageDifficulty += tempMaxDifficulty;
			}
			averageDifficulty = averageDifficulty / noteDifficultyArray.GetLength(0);
			averageDifficulty = averageDifficulty * (CalculateDifficulty.Clamp((Math.Log(noteDifficultyArray[noteDifficultyArray.GetLength(0) - 1, 0] / 1000 / 120) / 2), 1, 999999));
			averageDifficulty /= 4;
			maxDifficulty /= 4;
			overallDifficulty = averageDifficulty * ((1 - (averageDifficulty / (maxDifficulty))) + 1);
			overallDifficulty += (maxDifficulty - overallDifficulty) / 1.5;
			overallDifficulty = Math.Round(overallDifficulty, 2);
			averageDifficulty = Math.Round(averageDifficulty, 2);
			maxDifficulty = Math.Round(maxDifficulty, 2);
			MapInfo theData = new MapInfo();
			theData.MapName = mapName;
			theData.OverallDifficulty = overallDifficulty;
			theData.AverageDifficulty = averageDifficulty;
			theData.MaxDifficulty = maxDifficulty;
			theData.mapdata = mapdata;
			return theData;
		}
	}

	public class MapInfo
	{
		public string MapName { get; set; }
		public double OverallDifficulty { get; set; }
		public double AverageDifficulty { get; set; }
		public double MaxDifficulty { get; set; }
		public int index { get; set; }
		public string mapdata { get; set; }
    }
}
