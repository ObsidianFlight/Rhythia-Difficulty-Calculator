using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhythia_Difficulty_Calculator__GUI_
{
    internal class CalculateDifficulty
    {
		//Difficulty controls.
		
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

		public static double NegativeLog(double value)
		{
			Console.WriteLine($"Negative Log {value}, {(Math.Log(value) + 1) * -1}");
			return (Math.Log(value) + 1) * -1;
        }

		//Dead cause I gave up on it and decided to merge a lot of the stuff I added into the old system, which is what ConvertMapNew is, this used to be called the same thing
		//so some remnants may be laying around other parts of the code and I just comment it out.
		public static MapInfo ConvertMapDead(string mapdata, double speed, bool hasName, string name, double SPM, double SD, double FM, double VM, double BM, double noteSize)
        {
			Console.WriteLine("Running Settings");
			double speedPowMultiplier = SPM;
			double speedDivisor = SD;
			double flowMultiplier = FM;
			double velocityMultiplier = VM;
			double burstMultiplier = BM;

			double x1 = 0;
			double x2 = 0;
			double x3 = 0;
			double y1 = 0;
			double y2 = 0;
			double y3 = 0;
			double clockwise = 0;

			Console.WriteLine("Running Map Data");
			//Splits mapdata into an array so I can use it's information
			string[] mapraw = mapdata.Split(',');
			string mapName = mapraw[0];
			if (hasName == true)
			{
				mapName = name;
			}
			string[] notesraw = new string[mapraw.Length - 1];
			double[,] notearray = new double[notesraw.Length, 3];
			double[,] noteDifficultyArray = new double[notesraw.Length, 16];
			for (int i = 0; i < notesraw.Length; i++)
			{
				notesraw[i] = mapraw[i + 1];
				string[] data = notesraw[i].Split('|');
				notearray[i, 0] = Convert.ToDouble(data[0]);
				notearray[i, 1] = Convert.ToDouble(data[1]);
				notearray[i, 2] = Convert.ToDouble(data[2]) / speed;
			}
			bool pastStart = false;

			Console.WriteLine("Running Map Information");
			//Grabs all the information I need from the notes, Most likely done in an unefficient manner.
			for (int i = 0; i < notearray.GetLength(0); i++)
			{
				double timeSinceLastNote = 0;
				double distance = 1;
				double distanceFromCenter = 2;
				x1 = notearray[i, 0];
				y1 = notearray[i, 1];
				noteDifficultyArray[i, 10] = x1;
				noteDifficultyArray[i, 11] = y1;
				if (i != 0)
				{
					x2 = notearray[i - 1, 0];
					y2 = notearray[i - 1, 1];
					timeSinceLastNote = notearray[i, 2] - notearray[i - 1, 2];
				}
				noteDifficultyArray[i, 7] = timeSinceLastNote;
				double x = Math.Abs(x1 - x2);
				double y = Math.Abs(y1 - y2);
				double cx = Math.Abs(x1 - 1);
				double cy = Math.Abs(y1 - 1);
				distance = Math.Sqrt(x * x + y * y);
				noteDifficultyArray[i, 5] = distance;
				if (timeSinceLastNote > 0)
				{
					noteDifficultyArray[i, 1] = distance / timeSinceLastNote * 1000;
				} //Speed
				else { noteDifficultyArray[i, 1] = 0; }
				if (i > 0)
                {
					int partMega = 0;
					int stacks = 0;
					bool contStack = true;
					bool contMega = true;
					for (int j = 0; j < Clamp(i, 0, 6); j++)
					{
						if (noteDifficultyArray[i - j, 5] == 0 && contStack == true)
                        {
							stacks++;
							noteDifficultyArray[i, 1] = noteDifficultyArray[i - 1, 1] / Clamp(2.1 - (noteDifficultyArray[i, 7] / noteDifficultyArray[i - 1, 7]), 0.95 + (noteDifficultyArray[i-1, 8] / 50), 999);
							//Console.WriteLine($"{i + 1} is a stack with {i - j}");
                        }
                        else 
						{ 
							contStack = false; 
							//Console.WriteLine($"{i + 1}, is not a stack with {i + 1 - j}");
						}
						if (noteDifficultyArray[i-j, 7] == 0 && contMega == true)
                        {
							partMega++;
							stacks = 0;
                        }
						else { contMega = false; }
                    }
					noteDifficultyArray[i, 8] = stacks;
					noteDifficultyArray[i, 9] = partMega;
                }
				distanceFromCenter = Math.Sqrt(cx * cx + cy * cy);
				double direction = 12300;
				if (i > 1)
				{
					//"Just use the dot product of (blah blah)"
					//I KNOW!! I don't know how. I hope you like if statements.
					//Finds whether the last 3 notes are clockwise or not.
					int howBig1 = 0;
					int howBig2 = 0;
					int meganoteSize = 0;
					if (!pastStart)
					{
						if (noteDifficultyArray[1, 9] > 0)
						{
							direction = 0;
							howBig1 = 0;
							meganoteSize++;
							while (meganoteSize > howBig1)
							{
								if (meganoteSize + 1 == noteDifficultyArray[1 + meganoteSize, 9])
								{
									meganoteSize++;
									howBig1++;
								}
								else
								{
									howBig1++;
								}
							}
						}
						meganoteSize = 0;
						if (noteDifficultyArray[2 + howBig1, 9] > 0)
						{
							direction = 0;
							howBig2 = 0;
							meganoteSize++;
							while (meganoteSize > howBig2)
							{
								if (meganoteSize + 1 == noteDifficultyArray[2 + meganoteSize + howBig1, 9])
								{
									meganoteSize++;
									howBig2++;
								}
								else
								{
									howBig2++;
								}
							}
						}
					}

					double rotation = 0;
					double answer = 0;
					if (pastStart || i >= howBig1+howBig2 + 2)
                    {
						pastStart = true;
						x1 = noteDifficultyArray[i - 1 - (int)noteDifficultyArray[i - 1, 9] - 1 - (int)noteDifficultyArray[i - 1 - (int)noteDifficultyArray[i - 1, 9] - 1, 9], 10];
						x2 = noteDifficultyArray[i - 1 - (int)noteDifficultyArray[i - 1, 9], 10];
						x3 = noteDifficultyArray[i, 10];
						y1 = noteDifficultyArray[i - 1 - (int)noteDifficultyArray[i - 1, 9] - 1 - (int)noteDifficultyArray[i - 1 - (int)noteDifficultyArray[i - 1, 9] - 1, 9], 11];
						y2 = noteDifficultyArray[i - 1 - (int)noteDifficultyArray[i - 1, 9], 11];
						y3 = noteDifficultyArray[i, 11];

						double a1 = x1 - x2;
						double a2 = y1 - y2;
						double b1 = x3 - x2;
						double b2 = y3 - y2;
						double c1 = x2 - x3;
						double c2 = y3 - y2;
						if (c1 == 0 & c2 > 0) { direction = 90; }
						if (c1 == 0 & c2 < 0) { direction = 270; }
						if (c1 > 0 & c2 == 0) { direction = 0; }
						if (c1 == 0 & c2 == 0) { direction = 0; }
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

						rotation = Math.Acos(((a1 * b1) + (a2 * b2)) / (Math.Sqrt(a1 * a1 + a2 * a2) * Math.Sqrt(b1 * b1 + b2 * b2)));
						answer = Math.Round(rotation * (180 / Math.PI), 4);
					}
					noteDifficultyArray[i, 15] = answer;
				}
				noteDifficultyArray[i, 0] = notearray[i, 2]; //Time in song in milliseconds
				noteDifficultyArray[i, 2] = direction; //only used for rotation/flow detection, not that I put much thought into it.
				
				
				if (pastStart)
				{
					double n = noteDifficultyArray[(int)(i - 2 - noteDifficultyArray[(int)(i - 1 - noteDifficultyArray[i, 9]), 9]), 2];
					if (n > 180) { n -= 360; }
					double m = noteDifficultyArray[(int)(i - 1 - noteDifficultyArray[i, 9]), 2];
					if (m > 180) { m -= 360; }
					double k = 0;
					if (n == 0) { k = m; } else { k = m - n; }
					if (k < 0) { k += 360; }
					if (k > 0 & k < 180) { clockwise = k / 3; }
					if (k == 0 || k == 180) { clockwise = 0; }
					if (k > 180) { clockwise = k/3; }
				}
				noteDifficultyArray[i, 3] = clockwise; // 0 = straight, 1 = clockwise, -1 = counterclockwise,
				noteDifficultyArray[i, 4] = timeSinceLastNote / distance; // 1 distance = time
				if (pastStart)
				{
					noteDifficultyArray[i, 12] = noteDifficultyArray[i - (int)noteDifficultyArray[i, 9], 1] - noteDifficultyArray[i - (int)noteDifficultyArray[i, 9] - 1 - (int)noteDifficultyArray[i - 1 - (int)noteDifficultyArray[i, 9], 9], 1];
				} // Velocity

				if (i < 100)
				{
					double burstDistance = 0;
					for (int j = 0; j < i; j++)
					{
                        if (noteDifficultyArray[i-j, 0] > noteDifficultyArray[i, 0] - (noteDifficultyArray[i, 4] * 9) && noteDifficultyArray[i-j, 9] == 0)
                        {
							burstDistance += noteDifficultyArray[i - j, 5];
                        }
					}
					noteDifficultyArray[i, 6] = 1 + (1 / Clamp(8 - burstDistance, 1, 9999) / burstMultiplier); //note to self: see if this is actually helpful later.
				}
				else
				{
					double burstDistance = 0;
					for (int j = 0; j < 100; j++)
					{
						if (noteDifficultyArray[i - j, 0] > noteDifficultyArray[i, 0] - (noteDifficultyArray[i, 4] * 9) && noteDifficultyArray[i - j, 9] == 0)
						{
							burstDistance += noteDifficultyArray[i - j, 5];
						}
					}
					noteDifficultyArray[i, 6] = 1 + (1 / Clamp(8 - burstDistance, 1, 9999) / burstMultiplier); //note to self: see if this is actually helpful later.
				}
			}

			double[] flowChecker = new double[noteDifficultyArray.GetLength(0)];
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				if (i == 0 || i == 1) { flowChecker[i] = 1; }
				else
				{
					double u = noteDifficultyArray[i - 2, 3];
					double h = noteDifficultyArray[i - 1, 3];
					double b = noteDifficultyArray[i, 3];
					//Console.WriteLine($"{u}, {h}, {b}");

					//Most likely when you are spinning
					if (u > 0 & h > 0 & b > 0) { if (noteDifficultyArray[i, 5] > 1.5) { flowChecker[i] = 0; } else { flowChecker[i] = 10; } }
					else if (u < 0 & h < 0 & b < 0) { if (noteDifficultyArray[i, 5] > 1.5) { flowChecker[i] = 0; } else { flowChecker[i] = 10; } }
					//Quantum Slider, or repetitive back and forth jumps/slides
					//If Angle is 0, Uses 1 instead.
					else if (u == 0 & h == 0 & b == 0) { flowChecker[i] = 12; }
					//Coming out of repetitive back and forth jumps, or a quantum slider
					else if (u == 0 & h == 0 & b < 0) { if (noteDifficultyArray[i, 5] > 1.5) { flowChecker[i] = 5; } else { flowChecker[i] = 2; } }
					else if (u == 0 & h == 0 & b > 0) { if (noteDifficultyArray[i, 5] > 1.5) { flowChecker[i] = 5; } else { flowChecker[i] = 2; } }
					//The Wiggle Patterns.
					else if (u > 0 & h < 0 & b > 0) { flowChecker[i] = 7; }
					else if (u < 0 & h > 0 & b < 0) { flowChecker[i] = 7; }
					//Most likely when flow is reversed, or the start of a slider, if flow is hard reversed the angle should be 0.
					else if (u < 0 & h < 0 & b == 0) { if (noteDifficultyArray[i, 2] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u > 0 & h > 0 & b == 0) { if (noteDifficultyArray[i, 2] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u > 0 & h < 0 & b == 0) { if (noteDifficultyArray[i, 2] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 2; } }
					else if (u < 0 & h > 0 & b == 0) { if (noteDifficultyArray[i, 2] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 2; } }
					//Also reverses flow, I'm not sure how much emphasis I should be putting on these.
					else if (u > 0 & h > 0 & b < 0) { flowChecker[i] = 6; }
					else if (u < 0 & h < 0 & b > 0) { flowChecker[i] = 6; }
					else if (u > 0 & h < 0 & b < 0) { flowChecker[i] = 3; }
					else if (u < 0 & h > 0 & b > 0) { flowChecker[i] = 3; }
					//Most likely found in patterns that include Slides
					else if (u > 0 & h == 0 & b < 0) { flowChecker[i] = 4; }
					else if (u < 0 & h == 0 & b > 0) { flowChecker[i] = 4; }
					//Most likely in Spin Patterns, if not detected in a spin, increase by 3
					else if (u > 0 & h == 0 & u > 0)
					{
						if (noteDifficultyArray[i, 2] < 60)
						{
							flowChecker[i] = 4;
						}
						else
						{
							flowChecker[i] = 1;
						}
					}
					else if (u < 0 & h == 0 & u > 0) { if (noteDifficultyArray[i, 2] < 60) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 0 & h > 0 & b == 0) { if (noteDifficultyArray[i, 2] < 60) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 0 & h < 0 & b == 0) { if (noteDifficultyArray[i, 2] < 60) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else { flowChecker[i] = 1; }
					flowChecker[i] = flowChecker[i] / Clamp(noteDifficultyArray[i, 5], 1, 9999);
				}
			}

			Console.WriteLine("Running Individual Note Difficulty");
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				//Console.WriteLine("Start of Individual Note Difficulty");
				double individualNoteDifficulty = noteDifficultyArray[i, 1]; // Difficulty = Speed
				double cmdnsp = individualNoteDifficulty;
				individualNoteDifficulty = Math.Pow(individualNoteDifficulty, speedPowMultiplier); // Difficulty to the power of 3, since difficulty in speed is exponential
				double cmdpow = individualNoteDifficulty;
				individualNoteDifficulty /= speedDivisor; //Turns it into a more digestable number, 27000 turns into roughly 170
				double cmddvs = individualNoteDifficulty;
				individualNoteDifficulty *= 1 + (flowChecker[i] * flowMultiplier / 10 ); // I'm sure this matters! Flow detector? I'm sure it helped last time, right?
				double cmdfdt = individualNoteDifficulty;
				double cmdvcc = 0;
				/*if (velocityMultiplier != 0)
				{
                    //Console.WriteLine($"BV {i + 1}, {individualNoteDifficulty}, {noteDifficultyArray[i, 13]}, *{velocityMultiplier}");
					individualNoteDifficulty *= noteDifficultyArray[i, 13] * velocityMultiplier; // Difficulty based on velocity changes
					cmdvcc = individualNoteDifficulty;
					//Console.WriteLine($"BV {i + 1}, {individualNoteDifficulty}");
				}*/ //This is broken, Don't try to fix it.
				double cmdbmp = 0;
				if (burstMultiplier != 0)
				{
					individualNoteDifficulty /= noteDifficultyArray[i, 6]; // Difficulty reduced if it is a burst. This also reduces the start of fast sections but if they aren't a burst they make up for it anyway.
					cmdbmp = individualNoteDifficulty;
				}
				//Console.WriteLine("Middle of Individual Note Difficulty");
				double difficultyNoteSize = Clamp(noteSize, 0.2, 6);
				difficultyNoteSize *= 0.877192;
				difficultyNoteSize = Math.Pow(difficultyNoteSize, 2);
				difficultyNoteSize = 1 / difficultyNoteSize;
				//Console.WriteLine("Middle +1");
                if (noteDifficultyArray[i, 10] > (0.2 + ((noteSize - 1.13)/2)) & noteDifficultyArray[i, 10] < (1.8 - ((noteSize - 1.13) / 2))) //Increased difficulty if note
                {
					individualNoteDifficulty *= 1.4;
                }
				if (noteDifficultyArray[i, 11] > (0.2 + ((noteSize - 1.13) / 2)) & noteDifficultyArray[i, 11] < (1.8 - ((noteSize - 1.13) / 2))) // is not touching a wall
				{
					individualNoteDifficulty *= 1.4;
				}
				double cmdwal = individualNoteDifficulty;
				individualNoteDifficulty *= Clamp(2.4 - (1 / (180 / Clamp(180 - noteDifficultyArray[i, 15], 25, 180))), 1, 2.4);
				//Console.WriteLine("Middle +2");
				//individualNoteDifficulty *= (difficultyNoteSize / 2);
				//double cmdnts = individualNoteDifficulty;
				noteDifficultyArray[i, 14] = individualNoteDifficulty;
				//Console.WriteLine($"Note Number: {i + 1}, Final: {Math.Round(cmdwal, 2)} Speed: {Math.Round(cmdnsp, 2)}, Power: {Math.Round(cmdpow, 2)}, Divide: {Math.Round(cmddvs, 2)}, Flow: {Math.Round(cmdfdt, 2)}, Burst: {Math.Round(cmdbmp, 2)}");
				//Console.WriteLine("End of Individual Note Difficulty");
			}

			Console.WriteLine("Running Area Note Difficulty");
			int notesBehind = 0;
			double[] tempdifficulty = new double[noteDifficultyArray.GetLength(0)];
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				//Console.WriteLine("Start of Area Note Difficulty");
				notesBehind++;

				double[,] tempDiff = new double[notesBehind, 2];
				for (int j = 0; j < tempDiff.GetLength(0); j++)
				{
					tempDiff[j, 0] = noteDifficultyArray[i - j, 0]; //Gets Time in Song
					tempDiff[j, 1] = noteDifficultyArray[i - j, 14]; //Gets Difficulty
					if (tempDiff[j, 0] < noteDifficultyArray[i, 0] - 4000)
					{
						notesBehind--;
					}
				}
				//Console.WriteLine("Middle of Area Note Difficulty");
				double[] areaDifficultyArray = new double[tempDiff.GetLength(0)];
				for (int j = 0; j < tempDiff.GetLength(0); j++)
				{
					areaDifficultyArray[j] = tempDiff[j, 1];
				}
				Array.Sort(areaDifficultyArray);
				double areaDifficulty = 0;
				double areaIndex = 0;
				//Console.WriteLine("Middle +1");
				for (int j = 0; j < areaDifficultyArray.Length; j++)
				{
					int separator = (int)Math.Ceiling(areaDifficultyArray.Length * .25);
					int highSeparator = (int)Math.Floor(areaDifficultyArray.Length * .9);
					if (j < separator || j > highSeparator) { }
					else
					{
						areaDifficulty += areaDifficultyArray[j];
						areaIndex++;
					}
				}
				//Console.WriteLine("End");
				areaDifficulty /= areaIndex;
				tempdifficulty[i] = areaDifficulty;
			}

			Console.WriteLine("Running Low/High Difficulty");
			double lowDifficulty = 0;
			int lowIndex = 0;
			double highDifficulty = 0;
			int highIndex = 0;
			double maxDifficulty = 0;
			double overallDifficulty = 0;
			//Console.WriteLine("Start of Low/High Difficulty");
			Array.Sort(tempdifficulty);
			for (int i = 0; i < tempdifficulty.Length; i++)
			{
				//Console.WriteLine($"{i + 1}, {tempdifficulty[i]}");
				//Console.WriteLine("Middle");
				int separator = (int)Math.Ceiling(tempdifficulty.Length * .4);
				if (i < separator && i > 0)
				{
					lowDifficulty += tempdifficulty[i];
					//Console.WriteLine(tempdifficulty[i]);
					lowIndex++;
				}
				else if (i > 0)
				{
					highDifficulty += tempdifficulty[i];
					highIndex++;
				}
			}
			//Console.WriteLine("End");
			lowDifficulty /= lowIndex;
			highDifficulty /= highIndex;
			maxDifficulty = tempdifficulty[tempdifficulty.Length-1];
			overallDifficulty = maxDifficulty - ((maxDifficulty - highDifficulty) * .8);

			Console.WriteLine("Returning Map Data");
			MapInfo theData = new MapInfo();
			theData.MapName = mapName;
			theData.OverallDifficulty = Math.Round(overallDifficulty, 2);
			theData.LowDifficulty = Math.Round(lowDifficulty, 2);
			theData.HighDifficulty = Math.Round(highDifficulty, 2);
			theData.MaxDifficulty = Math.Round(maxDifficulty, 2);
			theData.mapdata = mapdata;
			return theData;
		}

		public static MapInfo ConvertMapNew(string mapdata, double speed, bool hasName, string name)
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
			if (hasName == true)
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

					if (distance == 0)
					{
						rotation = 180 * (Math.PI / 180);
						stack += 1;
					}
					else
					{
						rotation = Math.Acos((((a1 * b1) + (a2 * b2)) / (Math.Sqrt((a1 * a1 + a2 * a2)) * Math.Sqrt((b1 * b1 + b2 * b2)))));
						stack = 0;
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
						consistencyTimer += Clamp(noteDifficultyArray[i, 1] / 2, 1, 1000);
					}
					else
					{
						consistencyTimer -= noteDifficultyArray[i, 1];
						consistencyTimer = Clamp(consistencyTimer, 0, 5000);
						noteDifficultyArray[i, 1] += consistencyTimer;
					}
					double distanceoverN = 0;
					if (i > 4)
					{
						distanceoverN += noteDifficultyArray[i, 2] + noteDifficultyArray[i - 1, 2] + noteDifficultyArray[i - 2, 2] + noteDifficultyArray[i - 3, 2] + noteDifficultyArray[i - 4, 2];
					}
					if(i > 0)
                    {
						if (noteDifficultyArray[i, 3] == 0 && noteDifficultyArray[i-1, 3] > 0)
                        {
							noteDifficultyArray[i, 1] += noteDifficultyArray[i - 1, 1] / 2;
                        }
					}//Increases time since last note if the previous note was a stack
					final = Math.Pow(Math.Pow(Clamp(noteDifficultyArray[i, 1] / 1000, 0.02, 9999), -.5), 3.5) / 3.5; // Difficulty Based on Time
                    if (noteDifficultyArray[i, 5] < 30 || 150 < noteDifficultyArray[i, 5])
                    {
                        if (noteDifficultyArray[i, 2] > 2.25)
                        {
							noteDifficultyArray[i, 2] = ((noteDifficultyArray[i, 2] - 2) * .4) + 2;
                        }
                    }
					final *= Math.Pow(noteDifficultyArray[i, 2], 1.5) / 3.4; // Difficulty Based on Distance
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

					if (noteDifficultyArray[i, 3] > 0) { final = prevFinal / (2.3 + (noteDifficultyArray[i, 3] * .7)); } //Difficulty Based on Stack
					if (noteDifficultyArray[i, 2] > 0.4 && noteDifficultyArray[i, 1] < 135) 
					{
						if (noteDifficultyArray[i, 5] < 45) 
						{
							noteDifficultyArray[i, 5] = 0;
						}
						final *= 2.5 - (noteDifficultyArray[i, 5] - 180) * 0.5 / 180 * (Math.Abs(Clamp(noteDifficultyArray[i, 2], 1, 2) - 0.7) * 3); 
					} //Difficulty based on angle
					final *= Clamp(1 + (1 - noteDifficultyArray[i, 8]), 1, 1.5); // Difficulty based on distance from center
					final *= Clamp(2.4 - noteDifficultyArray[i, 9], 1, 2); // Difficulty change for offgrid shit (IDK if it works I made this a long while ago)
					final += 0.5;
					final /= 2.4;
					if (Double.IsNaN(final) == true) { final = 1; }
					noteDifficultyArray[i, 7] = final ;
					prevFinal = final ;
					//Console.WriteLine(Math.Round(final, 2));
				}
				//Console.WriteLine(noteDifficultyArray[i, 7]);
			}
			//Console.WriteLine("");
			//Console.WriteLine("Next up: Area Difficulty");
			//Console.WriteLine("");
			int notesBehind = 0;
			double maxDifficulty = 0;
			int timeOfMaxDifficulty = 0;
			int noteNumberOfMaxDifficulty = 0;
			double prevTempDiff = 0;
			double averageDifficulty = 0;
			double[] noteDifficulty = new double[noteDifficultyArray.GetLength(0)];
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				if (notesBehind < 32) 
				{
					notesBehind++; 
				}
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
				//Console.WriteLine("1. Addition of all notes: " + tempMaxDifficulty);
				tempMaxDifficulty /= 4.5;
				//Console.WriteLine("2. Division by 4.5: " + tempMaxDifficulty);
				//if (tempMaxDifficulty > prevTempDiff)
				//{
				//	tempMaxDifficulty -= 5 * Math.Log(tempMaxDifficulty - prevTempDiff);
					//Console.WriteLine("3. Not really sure what this does: " + tempMaxDifficulty);
				//}
				if (tempMaxDifficulty > maxDifficulty)
				{
					maxDifficulty = tempMaxDifficulty;
					timeOfMaxDifficulty = (int)noteDifficultyArray[i, 0];
					noteNumberOfMaxDifficulty = i + 1;
				}
				//Console.WriteLine($"{i + 1}, {Math.Round(tempMaxDifficulty, 2)}");
				prevTempDiff = tempMaxDifficulty;
				averageDifficulty += tempMaxDifficulty;
				noteDifficulty[i] = tempMaxDifficulty;
			}
			Console.WriteLine($"Song: {mapName}, Max Difficulty: {Math.Round(maxDifficulty / 4, 2)} At: {timeOfMaxDifficulty}, Note: {noteNumberOfMaxDifficulty}");
			double overallDifficulty;
			double highDifficulty = 0;
			int highIndex = 0;
			double lowDifficulty = 0;
			int lowIndex = 0;
			Array.Sort(noteDifficulty);
			for(int i = 0; i < noteDifficulty.Length; i++)
            {
				if(i < Math.Ceiling(noteDifficulty.Length * .45))
                {
					lowDifficulty += noteDifficulty[i];
					lowIndex++;
                }
                else
                {
					highDifficulty += noteDifficulty[i];
					highIndex++;
                }
            }
			lowDifficulty /= lowIndex;
			highDifficulty /= highIndex;
			averageDifficulty = averageDifficulty / noteDifficultyArray.GetLength(0);

			lowDifficulty /= 4;
			highDifficulty /= 4;
			averageDifficulty /= 4;
			maxDifficulty /= 4;
			lowDifficulty *= 0.8;
			highDifficulty *= 0.8;
			averageDifficulty *= 0.8;
			maxDifficulty *= 0.8;
			overallDifficulty = highDifficulty * (1 - (highDifficulty / maxDifficulty) + 1);
			overallDifficulty += (maxDifficulty - overallDifficulty) / 1.5;
			overallDifficulty = Math.Round(overallDifficulty, 2);
			averageDifficulty = Math.Round(averageDifficulty, 2);
			lowDifficulty = Math.Round(lowDifficulty, 2);
			highDifficulty = Math.Round(highDifficulty, 2);
			maxDifficulty = Math.Round(maxDifficulty, 2);
			MapInfo theData = new MapInfo();
			theData.MapName = mapName;
			theData.OverallDifficulty = overallDifficulty;
			theData.AverageDifficulty = averageDifficulty;
			theData.LowDifficulty = lowDifficulty;
			theData.HighDifficulty = highDifficulty;
			theData.MaxDifficulty = maxDifficulty;
			theData.mapdata = mapdata;
			return theData;
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
					
					if (distance == 0)
					{
						rotation = 180 * (Math.PI / 180);
						stack += 1;
					}
					else
					{
						rotation = Math.Acos((((a1 * b1) + (a2 * b2)) / (Math.Sqrt((a1 * a1 + a2 * a2)) * Math.Sqrt((b1 * b1 + b2 * b2)))));
						stack = 0;
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
		public double LowDifficulty { get; set; }
		public double HighDifficulty { get; set; }
		public int index { get; set; }
		public string mapdata { get; set; }
    }
}
