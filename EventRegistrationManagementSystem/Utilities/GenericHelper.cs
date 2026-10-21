namespace EventRegistrationManagementSystem.Utilities
{
    public static class GenericHelper
    {
        public static List<T> Filter<T>(
            IEnumerable<T> items,
            Predicate<T> condition)
        {
            List<T> result = new();

            foreach (T item in items)
            {
                if (condition(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}