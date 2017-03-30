namespace Wikiled.Arff.Persistence.Headers
{
    public interface IClassHeader : IHeader
    {
        int ReadClassIdValue(DataRecord record);

        object GetValueByClassId(int classId);
    }
}
