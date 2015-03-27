
namespace EducationSystem.Detectors
{
    abstract class AbstractDetector<I, O>
    {
        public abstract O decide(I input);
    }
}
