using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

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

		public static string Parse(string path)
		{
			using FileStream data = new(path, FileMode.Open, FileAccess.Read);
			data.Seek(0, SeekOrigin.Begin);

			using BinaryReader reader = new(data);
			string? mapData = null;

			string fileTypeSignature = Encoding.ASCII.GetString(reader.ReadBytes(4));

			if (fileTypeSignature != "SS+m")
			{
				MessageBox.Show("File type not recognized or supported\nCurrently supported: SSPM v1/v2", "OK");
				return "";
			}

			ushort formatVersion = reader.ReadUInt16();

			if (formatVersion == 1)
			{
				string GetNextVariableString()
				{
					List<byte> bytes = new();
					byte cur = reader.ReadByte();

					while (cur != 0x0a)
					{
						bytes.Add(cur);
						cur = reader.ReadByte();
					}

					return Encoding.UTF8.GetString(bytes.ToArray());
				}


				// v1
				reader.ReadBytes(2);

				// metadata
				string mapID = GetNextVariableString();
				string mapName = GetNextVariableString();
				string mappers = GetNextVariableString();

				uint lastMs = reader.ReadUInt32();
				uint noteCount = reader.ReadUInt32();
				byte difficulty = reader.ReadByte();


				// read cover
				byte containsCover = reader.ReadByte(); // sspm v1 has 0x02 as PNG so it cant be parsed as a bool


				if (containsCover == 0x02)
				{
					ulong coverLength = reader.ReadUInt64();
					byte[] cover = reader.ReadBytes((int)coverLength);
				}

				// read audio
				bool containsAudio = reader.ReadBoolean();

				if (containsAudio)
				{
					ulong audioLength = reader.ReadUInt64();
					byte[] audio = reader.ReadBytes((int)audioLength);

				}

				mapData = mapID;

				// read notes
				string[] notes = new string[noteCount];

				for (uint i = 0; i < noteCount; i++)
				{
					uint ms = reader.ReadUInt32();
					bool isQuantum = reader.ReadBoolean();

					float x;
					float y;

					if (isQuantum)
					{
						x = reader.ReadSingle();
						y = reader.ReadSingle();
					}
					else
					{
						x = reader.ReadByte();
						y = reader.ReadByte();
					}

					notes[i] = $",{2 - x}|{2 - y}|{ms}";
				}

				mapData += string.Join("", notes);
			}
			else if (formatVersion == 2)
			{
				string GetNextVariableString(bool fourBytes = false)
				{
					uint length = fourBytes ? reader.ReadUInt32() : reader.ReadUInt16();
					byte[] str = reader.ReadBytes((int)length);

					return Encoding.UTF8.GetString(str);
				}


				// v2
				reader.ReadBytes(4);

				// metadata
				byte[] hash = reader.ReadBytes(20);
				uint lastMs = reader.ReadUInt32();
				uint noteCount = reader.ReadUInt32();
				uint markerCount = reader.ReadUInt32();

				byte difficulty = reader.ReadByte();
				ushort mapRating = reader.ReadUInt16();
				bool containsAudio = reader.ReadBoolean();
				bool containsCover = reader.ReadBoolean();
				bool requiresMod = reader.ReadBoolean();

				// pointers
				ulong customDataOffset = reader.ReadUInt64();
				ulong customDataLength = reader.ReadUInt64();
				ulong audioOffset = reader.ReadUInt64();
				ulong audioLength = reader.ReadUInt64();
				ulong coverOffset = reader.ReadUInt64();
				ulong coverLength = reader.ReadUInt64();
				ulong markerDefinitionsOffset = reader.ReadUInt64();
				ulong markerDefinitionsLength = reader.ReadUInt64();
				ulong markerOffset = reader.ReadUInt64();
				ulong markerLength = reader.ReadUInt64();

				// get song name stuff and mappers
				string mapID = GetNextVariableString();
				string mapName = GetNextVariableString();
				string songName = GetNextVariableString();

				ushort mapperCount = reader.ReadUInt16();
				string[] mappers = new string[mapperCount];

				for (ushort i = 0; i < mapperCount; i++)
					mappers[i] = GetNextVariableString();


				// read custom data block
				// may implement more fields in the future, but right now only 'difficulty_name' is used

				// jump to beginning of audio block in case custom data reading was unsuccessful
				reader.BaseStream.Seek((long)audioOffset, SeekOrigin.Begin);

				// read and cache audio
				if (containsAudio)
				{
					byte[] audio = reader.ReadBytes((int)audioLength);
				}


				// read cover
				if (containsCover)
				{
					byte[] cover = reader.ReadBytes((int)coverLength);
				}

				mapData = mapID;

				// read marker definitions
				bool hasNotes = false;

				byte numDefinitions = reader.ReadByte();

				for (byte i = 0; i < numDefinitions; i++)
				{
					string definition = GetNextVariableString();
					hasNotes |= definition == "ssp_note" && i == 0;

					byte numValues = reader.ReadByte();

					byte definitionData = 0x01;
					while (definitionData != 0x00)
						definitionData = reader.ReadByte();
				}

				if (!hasNotes)
					return mapData;

				// process notes
				string[] notes = new string[noteCount];

				for (uint i = 0; i < noteCount; i++)
				{
					uint ms = reader.ReadUInt32();
					byte markerType = reader.ReadByte();
					bool isQuantum = reader.ReadBoolean();

					float x;
					float y;

					if (isQuantum)
					{
						x = reader.ReadSingle();
						y = reader.ReadSingle();
					}
					else
					{
						x = reader.ReadByte();
						y = reader.ReadByte();
					}

					notes[i] = $",{(2 - x).ToString()}|{(2 - y).ToString()}|{ms}";
				}

				mapData += string.Join("", notes);
			}
			else
				MessageBox.Show("File version not recognized or supported\nCurrently supported: SSPM v1/v2", "OK");

			return mapData ?? "";
		}

		public static double NegativeLog(double value)
		{
			Console.WriteLine($"Negative Log {value}, {(Math.Log(value) + 1) * -1}");
			return (Math.Log(value) + 1) * -1;
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
			Note[] theNoteList = new Note[notesraw.Length];
			double[,] notearray = new double[notesraw.Length, 3];
			double[,] noteDifficultyArray = new double[notesraw.Length, 11];
			for (int i = 0; i < notesraw.Length; i++)
			{
				notesraw[i] = mapraw[i + 1];
				string[] data = notesraw[i].Split('|');
                theNoteList[i] = new Note(Convert.ToDouble(data[0]), Convert.ToDouble(data[1]), Convert.ToDouble(data[2]) / speed);
			}
			var orderedNoteList = theNoteList.OrderBy(x => x.Ms);
			int pp = 0;
			foreach (var n in orderedNoteList)
            {
				notearray[pp, 0] = n.X;
				notearray[pp, 1] = n.Y;
                notearray[pp, 2] = n.Ms;
				pp++;
			}
			int stack = 0;
			//Grabs all the information I need from the notes, Most likely done in an unefficient manner.
			for (int i = 0; i < notearray.GetLength(0); i++)
			{
				noteDifficultyArray[i, 10] = 0;
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

			float[] flowChecker = new float[noteDifficultyArray.GetLength(0)];
			for (int i = 0; i < noteDifficultyArray.GetLength(0); i++)
			{
				if (i == 0 || i == 1) { flowChecker[i] = 2; }
				else
				{
					int u = (int)noteDifficultyArray[i - 2, 6];
					int h = (int)noteDifficultyArray[i - 1, 6];
					int b = (int)noteDifficultyArray[i, 6];

					//Most likely when you are spinning
					if (u == 1 & h == 1 & b == 1) 
					{
						if (i > 1)
						{
							if (notearray[i, 0] < 1.5 && notearray[i, 0] > 0.5)
							{
								flowChecker[i] = 2.5f;
                                if (noteDifficultyArray[i, 2] > 1.7)
                                {
                                    flowChecker[i] = 2;
                                }
							}
							else if (notearray[i, 1] < 1.5 && notearray[i, 1] > 0.5)
							{
								flowChecker[i] = 2.5f;
								if (noteDifficultyArray[i, 2] > 1.7)
								{
                                    flowChecker[i] = 2;
								}
							}
							if (notearray[i, 0] > 2 || notearray[i, 1] > 2 || notearray[i, 0] < 0 || notearray[i, 1] < 0)
                            {
                                flowChecker[i] = 0;
                            }
						}
						else { flowChecker[i] = 2; }
						noteDifficultyArray[i, 10] = 1 + noteDifficultyArray[i-1, 10];
					}
					else if (u == -1 & h == -1 & b == -1) {

						if (i > 1)
						{
							if (notearray[i, 0] < 1.5 && notearray[i, 0] > 0.5)
							{
								flowChecker[i] = 2.5f;
								if (noteDifficultyArray[i, 2] > 1.7)
								{
									flowChecker[i] = 3f;
								}
							}
							else if (notearray[i, 1] < 1.5 && notearray[i, 1] > 0.5)
							{
								flowChecker[i] = 2.5f;
								if (noteDifficultyArray[i, 2] > 1.7)
								{
									flowChecker[i] = 3f;
								}
							}
							if (notearray[i, 0] > 2 || notearray[i, 1] > 2 || notearray[i, 0] < 0 || notearray[i, 1] < 0)
							{
								flowChecker[i] = 0;
							}
						}
						else { flowChecker[i] = 2; }
						noteDifficultyArray[i, 10] = -1 + noteDifficultyArray[i - 1, 10];
					}
					//Quantum Slider, or repetitive back and forth jumps/slides
					//If Angle is 0, Uses 1 instead.
					else if (u == 0 & h == 0 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 1; } else { flowChecker[i] = 4; } }
					//Coming out of repetitive back and forth jumps, or a quantum slider
					else if (u == 0 & h == 0 & b == -1) { flowChecker[i] = 2; }
					else if (u == 0 & h == 0 & b == 1) { flowChecker[i] = 2; }
					//The Wiggle Patterns.
					else if (u == 1 & h == -1 & b == 1) { if (noteDifficultyArray[i, 2] > 1.7) { flowChecker[i] = 2; } else { flowChecker[i] = 1; } }
					else if (u == -1 & h == 1 & b == -1) { if (noteDifficultyArray[i, 2] > 1.7) { flowChecker[i] = 2; } else { flowChecker[i] = 1; } }
					//Most likely when flow is reversed, or the start of a slider, if flow is hard reversed the angle should be 0.
					else if (u == -1 & h == -1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 1 & h == 1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 1; } }
					else if (u == 1 & h == -1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 2; } }
					else if (u == -1 & h == 1 & b == 0) { if (noteDifficultyArray[i, 5] == 0) { flowChecker[i] = 4; } else { flowChecker[i] = 2; } }
					//Also reverses flow, I'm not sure how much emphasis I should be putting on these.
					else if (u == 1 & h == 1 & b == -1) { flowChecker[i] = 2; }
					else if (u == -1 & h == -1 & b == 1) { flowChecker[i] = 2; }
					else if (u == 1 & h == -1 & b == -1) { flowChecker[i] = 2; }
					else if (u == -1 & h == 1 & b == 1) { flowChecker[i] = 2; }
					//Most likely in Spin Patterns, if not detected in a spin, increase by 3
					else if (u == 1 & h == 0 & u == 1)
					{
						if (noteDifficultyArray[i, 5] < 60)
						{
							flowChecker[i] = 6;
						}
						else
						{
							flowChecker[i] = 2;
						}
						noteDifficultyArray[i, 10] = 1 + noteDifficultyArray[i - 1, 10];
					}
					else if (u == -1 & h == 0 & u == -1)
					{
						if (noteDifficultyArray[i, 5] < 60)
						{
							flowChecker[i] = 6;
						}
						else
						{
							flowChecker[i] = 2;
						}
						noteDifficultyArray[i, 10] = 1 + noteDifficultyArray[i - 1, 10];
					}
					else if (u == -1 & h == 0 & u == 1) { if (noteDifficultyArray[i, 5] < 60) { flowChecker[i] = 6; } else { flowChecker[i] = 2; } }
					else if (u == 0 & h == 1 & b == 0) { if (noteDifficultyArray[i, 5] < 60) { flowChecker[i] = 6; } else { flowChecker[i] = 2; } }
					else if (u == 0 & h == -1 & b == 0) { if (noteDifficultyArray[i, 5] < 60) { flowChecker[i] = 6; } else { flowChecker[i] = 2; } }
					else { flowChecker[i] = 1; }
					//Console.WriteLine($"{i}, {u} {h} {b}");
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
                    if (noteDifficultyArray[i, 2] > 1.42 && noteDifficultyArray[i, 2] < 2 && Math.Abs(noteDifficultyArray[i, 10]) < 6)
                    {
						noteDifficultyArray[i, 2] = 1.2;
                    } // Sets distance to 1.2 if under 2 distance, over 1.42 distance, and is a spin.
                    if (noteDifficultyArray[i, 2] < 2.83)
                    {
						final *= Math.Pow(noteDifficultyArray[i, 2], 1.5) / 3.4; // Difficulty Based on Distance
                    }
                    else
                    {
						final *= (Math.Pow(2.83, 1.5) / 3.4) + (Math.Pow(noteDifficultyArray[i, 2] - 1.83, 1.2) / 3.4);
                    }
					
					if (noteDifficultyArray[i, 2] < 1.1) { final *= 1.2; }
					else if (noteDifficultyArray[i, 2] < 1.8 && noteDifficultyArray[i, 5] > 70) { final *= 0.8; }
					else if (noteDifficultyArray[i, 2] > 1.9) { final *= 1.1; }
					if (noteDifficultyArray[i, 2] > 2.5) { final *= 0.8; }
					if (distanceoverN < 3 || noteDifficultyArray[i, 3] > 0) { final *= 0.85; } 
					else
					{
						double flow = 0;
						flow = flowChecker[i] * 0.5 * (1 + (noteDifficultyArray[i, 5] / 240));
						if (noteDifficultyArray[i, 2] > 1.2 && noteDifficultyArray[i, 2] < 2 && distanceoverN > 7 && Math.Abs(noteDifficultyArray[i, 10]) < 10) { flow *= 2; }
						if (Math.Abs(noteDifficultyArray[i, 10]) > 15) { flow = 0; }
						//Console.WriteLine(i + " " + noteDifficultyArray[i, 10]);
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
					final += 0.5;
					final /= 3;
					if (Double.IsNaN(final) == true) { final = 1; }
					noteDifficultyArray[i, 7] = final;
					prevFinal = final;
					//Console.WriteLine(Math.Round(final, 2));
				}
				//Console.WriteLine(noteDifficultyArray[i, 7]);
			}

			int noteAmountBehind = 0;
			for (int l = 0; l < noteDifficultyArray.GetLength(0); l++)
			{
				
				if (noteAmountBehind < 32)
				{
					noteAmountBehind++;
				}
				double[,] tempDist = new double[noteAmountBehind, 3];
				for (int j = 0; j < tempDist.GetLength(0); j++)
				{
					tempDist[j, 2] = noteDifficultyArray[l - j, 2]; //Gets Distance
					tempDist[j, 1] = noteDifficultyArray[l - j, 1]; //Gets Time Since Last Note
					tempDist[j, 0] = noteDifficultyArray[l - j, 0];
					if (tempDist[j, 0] < noteDifficultyArray[l, 0] - 2000)
					{
						noteAmountBehind--;
					}
				}
				if (l > 0)
				{
					
					double totalDist = 0;
					double totalTime = 0;
					int notesChecked = 0;
					while (totalDist < 7 && notesChecked < noteAmountBehind)
					{
						totalDist += tempDist[notesChecked, 2];
						totalTime += tempDist[notesChecked, 1];
						notesChecked++;
					}
					double totalSpeed = totalTime / totalDist;
					double currentSpeed = noteDifficultyArray[l, 1] / noteDifficultyArray[l, 2];
					//noteDifficultyArray[l, 7] *= Clamp(currentSpeed / totalSpeed, 0.85, 1);
				}
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
					if (tempDiff[j, 0] < noteDifficultyArray[i, 0] - 2000)
					{
						notesBehind--;
					}
				}
				double tempMaxDifficulty = 0;
				for (int j = 0; j < tempDiff.GetLength(0); j++)
				{
					tempMaxDifficulty += tempDiff[j, 1];
				}
				tempMaxDifficulty /= 4;
				if (tempMaxDifficulty > prevTempDiff)
				{
					tempMaxDifficulty -= Math.Log(tempMaxDifficulty - prevTempDiff);
				}
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

			lowDifficulty /= 2.6;
			highDifficulty /= 2.6;
			averageDifficulty /= 2.6;
			maxDifficulty /= 2.6;
			overallDifficulty = highDifficulty * (1 - (highDifficulty / maxDifficulty) + 1);
			overallDifficulty += (maxDifficulty - overallDifficulty) / 1.5;
			overallDifficulty = Math.Round(overallDifficulty, 2);
			averageDifficulty = Math.Round(averageDifficulty, 2);
			lowDifficulty = Math.Round(lowDifficulty, 2);
			highDifficulty = Math.Round(highDifficulty, 2);
			maxDifficulty = Math.Round(maxDifficulty, 2);
			MapInfo theData = new MapInfo();
			theData.MapName = mapName;
			theData.OverallDifficulty = Math.Round(ScaleNumber(overallDifficulty), 2);
			theData.AverageDifficulty = Math.Round(ScaleNumber(averageDifficulty), 2);
			theData.LowDifficulty = Math.Round(ScaleNumber(lowDifficulty), 2);
			theData.HighDifficulty = Math.Round(ScaleNumber(highDifficulty), 2);
			theData.MaxDifficulty = Math.Round(ScaleNumber(maxDifficulty), 2);
			theData.mapdata = mapdata;
			return theData;
		}

		//ChatGPT helped me with the idea for this cause I didn't know what it was called and brainstorming is hurting
		static double ScaleNumber(double input)
		{
			if (input > 150)
				return 7 + ((input - 150) / 50);
			else if (input > 110)
				return 6 + ((input - 110) / 40);
			else if (input > 40)
				return 5 + ((input - 40) / 70);
			else if (input > 15)
				return 4 + ((input - 15) / 25);
			else if (input > 10)
				return 3 + ((input - 10) / 5);
			else if (input > 5)
				return 2 + ((input - 5) / 5);
			else if (input > 2)
				return 1 + ((input - 2) / 3);
			else if (input >= 0)
				return input / 2;
			else
				return 0;
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
