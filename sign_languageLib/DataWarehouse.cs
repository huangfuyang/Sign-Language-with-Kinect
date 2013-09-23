using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

using LearningSystem.DataStorage;
using LearningSystem.StaticTools;

public class DataWarehouse
{
    private int _currentFrame;
    public int m_currentFrame
    {
        get { return _currentFrame; }
        private set { _currentFrame = value; }
    }

    private List<FrameData> _frameData;
    public List<FrameData> m_frameData
    {
        get { return _frameData; }
        set { _frameData = value; }
    }

    public DataWarehouse()
    {
        m_frameData = new List<FrameData>();
        m_frameData.Add(new FrameData(0));
        
    }
    public bool SetSkeletonFrameData(SkeletonFrame sf)
    {
        Skeleton[]  skeletons = new Skeleton[sf.SkeletonArrayLength];
        sf.CopySkeletonDataTo(skeletons);
        foreach (Skeleton sk in skeletons)
        {
            if (sk.TrackingState == SkeletonTrackingState.Tracked)
            {
                m_frameData.Add(new FrameData(++m_currentFrame));//first frame is frame 1
                m_frameData[m_currentFrame].m_Player1.m_position = UtilityTools.SkeletonPointToVector3(sk.Position);
                m_frameData[m_currentFrame].m_Player1.m_skeleton = sk;
                return true;
            }
            
        }
        return false;
       

    }

    public Vector3 GetPlayer1CurrentPosition()
    {
        return GetCurrentFrameData().m_Player1.m_position;
    }

    public FrameData GetCurrentFrameData()
    {
        if (m_currentFrame == 0)
        {
            //TODO: not recognize
            return m_frameData[m_currentFrame];
        }
        return m_frameData[m_currentFrame];
    }

}
