using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.Generators.Data
{
    public class GridData
    {
        public int x;
        public int y;

        public double height;
        public double temperature;
        public double humidity;
        public string biome;
    }

    public class GridWorldData
    {
        public int X { get; set; }
        public int Y { get; set; }

        public GridData[,] Data { get; private set; }

        public GridWorldData(int x, int y)
        {
            X = x;
            Y = y;
            Data = new GridData[Y, X];
            for (int y1 = 0; y1 < y; y1++)
            {
                for (int x1 = 0; x1 < x; x1++)
                {
                    Data[y1, x1].x = x1;
                    Data[y1, x1].y = y1;
                }
            }
        }

        public bool SetGrid(int x, int y, double height, double temperature, double humidity, string biome)
        {
            try
            {
                GridData data = Data[y, x];
                data.height = height;
                data.temperature = temperature;
                data.humidity = humidity;
                data.biome = biome;

                return true;
            } catch
            {
                return false;
            }
        }

        public bool SetGrid(int x, int y, GridData gridData)
        {
            gridData.x = x;
            gridData.y = y;
            try
            {
                Data[y, x] = gridData;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetHeightData(double[,] vals)
        {
            try
            {
                for (int y = 0; y < Y; y++)
                {
                    for (int x = 0; x < X; x++)
                    {
                        Data[y, x].height = vals[y, x];
                    }
                }

                return true;
            } catch
            {
                return false;
            }
        }

        public bool SetTemperatureData(double[,] vals)
        {
            try
            {
                for (int y = 0; y < Y; y++)
                {
                    for (int x = 0; x < X; x++)
                    {
                        Data[y, x].temperature = vals[y, x];
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetHumidityData(double[,] vals)
        {
            try
            {
                for (int y = 0; y < Y; y++)
                {
                    for (int x = 0; x < X; x++)
                    {
                        Data[y, x].humidity = vals[y, x];
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
