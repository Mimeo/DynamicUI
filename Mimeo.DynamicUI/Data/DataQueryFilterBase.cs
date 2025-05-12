namespace Mimeo.DynamicUI.Data
{
    public abstract class DataQueryFilterBase
    {
        public abstract DataQueryFilterBase Clone();

        public static bool operator ==(DataQueryFilterBase? left, DataQueryFilterBase? right)
        {
            if (left is null && right is null)
            {
                return true;
            }
            else if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(DataQueryFilterBase left, DataQueryFilterBase right)
        {
            return !(left == right);
        }

        public override abstract bool Equals(object? obj);

        public override abstract int GetHashCode();
    }
}
