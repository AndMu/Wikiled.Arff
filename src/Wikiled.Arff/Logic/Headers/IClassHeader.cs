namespace Wikiled.Arff.Logic.Headers
{
    public interface IClassHeader : IHeader
    {
        int ReadClassIdValue(DataRecord record);

        object GetValueByClassId(int classId);
    }
}
