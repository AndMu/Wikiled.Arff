namespace Wikiled.Arff.Logic
{
    public interface IInformationLine
    {
        void Add(int currentIndex, string value);

        void Add(string value);

        void MoveIndex(int index);

        string GenerateLine();

        int Index { get; }
    }
}