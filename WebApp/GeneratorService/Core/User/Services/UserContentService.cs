namespace GeneratorService.Core.User.Services;


public interface IUserContentService {
    Task<bool> CreateContentAsync();
    Task<bool> DeleteContentAsync();
    Task<bool> UpdateContentAsync();
}


public class UserContentService : IUserContentService {

}
