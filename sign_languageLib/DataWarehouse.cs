using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

using LearningSystem.DataStorage;
using LearningSystem.StaticTools;

public class DataWarehouse :ISubject
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

    /// <summary>
    /// set frame data to data warehouse and return whether the data is valid
    /// </summary>
    /// <param name="sf"></param>
    /// <returns>whether the data is successfully stored</returns>
    public bool SetSkeletonFrameData(SkeletonFrame sf)
    {
        Skeleton[] skeletons = new Skeleton[sf.SkeletonArrayLength];
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
        return GetCurrentFrameData() == null ? Vector3.Zero : GetCurrentFrameData().m_Player1.m_position;

    }

    public FrameData GetCurrentFrameData()
    {
        return m_currentFrame == 0 ? null : m_frameData[m_currentFrame];
    }

    public List<Vector3> GetLatestPlayer1Positions(int frames)
    {
        if (frames <= 0)
        {
            return null;
        }
        List<Vector3> results = new List<Vector3>(frames);
        if (frames < m_currentFrame)
        {
            List<FrameData> l = m_frameData.GetRange(m_currentFrame - frames, frames);
            l.ForEach(i => results.Add(i.m_Player1.m_position));
            return results;
        }
        else
        {
            List<FrameData> l = m_frameData.GetRange(1, m_currentFrame);
            l.ForEach(i => results.Add(i.m_Player1.m_position));
            return results;
        }

    }

    public List<Vector3> GetPlayer1PositionBetweenFrames(int start, int end)
    {
        if (end - start <= 0 || start <= 0)
        {
            return null;
        }
        List<Vector3> results = new List<Vector3>(end - start);

        List<FrameData> l = m_frameData.GetRange(start, Math.Min(m_currentFrame - start + 1, end - start +1));
        l.ForEach(i => results.Add(i.m_Player1.m_position));
        return results;

    }



    #region ISubject 成员

    public event DataTransferEventHandler m_dataTransferEvent;

    public void NofityAll(DataTransferEventArgs e)
    {
        throw new NotImplementedException();
    }

    #endregion
}
