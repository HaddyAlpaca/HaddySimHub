// This file is part of iRacingSDK.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/iRacingSDK.Net
//
// iRacingSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// iRacingSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with iRacingSDK.  If not, see <http://www.gnu.org/licenses/>.

namespace iRacingSDK
{
    public static class iRacing
    {
        static iRacingConnection instance;
        static iRacingEvents eventInstance;

        static iRacing()
        {
            instance = new iRacingConnection();
            eventInstance = new iRacingEvents();
        }

        public static Replay Replay => instance.Replay;
        public static PitCommand PitCommand => instance.PitCommand;

        public static bool IsConnected => instance.IsConnected;

        public static IEnumerable<DataSample> GetDataFeed() => instance.GetDataFeed();

        public static void StartListening()
        {
            eventInstance.StartListening();
        }

        public static void StopListening()
        {
            eventInstance.StopListening();
        }

        public static event Action<DataSample> NewData
        {
            add
            {
                eventInstance.NewData += value;
            }
            remove
            {
                eventInstance.NewData -= value;
            }
        }
    }
}
