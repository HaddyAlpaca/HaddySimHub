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

namespace iRacingSDK;

public partial class SessionData
{
    public partial class _SessionInfo
    {
        public partial class _Sessions
        {
            public int _SessionLaps
            {
                get
                {
                    int result = 0;
                    int.TryParse(SessionLaps, out result);
                    return result;
                }
            }

            public double _SessionTime
            {
                get
                {
                    double result = 0;
                    double.TryParse(SessionTime.Replace(" sec", ""), out result);
                    return result;
                }
            }

            public bool IsLimitedSessionLaps => SessionLaps.ToLower() != "unlimited";

            public bool IsLimitedTime => SessionTime.ToLower() != "unlimited";
        }
    }
	}
