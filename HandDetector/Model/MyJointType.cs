using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CURELab.SignLanguage.HandDetector.Model
{
    // Summary:
    //     This contains all of the possible joint types.
    public enum MyJointType
    {

        Head = 0,
        ShoulderLeft = 1,
       
        ShoulderCenter = 2,

        ShoulderRight = 3,

        ElbowL = 4,

        ElbowR = 5,

        WristL = 6,

        WristR = 7,

        HandL = 8,
      
        HandR = 9,

        Spine = 10,

        HipL = 11,

        HipCenter = 12,

        HipR = 13,

        AnkleLeft = 14,

        FootLeft = 15,

        //
        // Summary:
        //     The right knee.
        KneeRight = 17,
        //
        // Summary:
        //     The right ankle.
        AnkleRight = 18,
        //
        // Summary:
        //     The right foot.
        FootRight = 19,
    }

}
