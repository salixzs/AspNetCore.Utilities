using System.Threading.Tasks;

namespace Sample.AspNet5.Logic
{
    public interface ISampleLogic
    {
        /// <summary>
        /// Dummy method in business domain logic, which ends up throwing exception.
        /// </summary>
        Task FaultyLogic();

        /// <summary>
        /// Dummy method in business domain logic, which ends up throwing database exception.
        /// </summary>
        Task DatabaseProblemLogic();

        /// <summary>
        /// Dummy method in business domain logic, which ends up throwing data validation exception.
        /// </summary>
        Task ValidationError();
    }
}
