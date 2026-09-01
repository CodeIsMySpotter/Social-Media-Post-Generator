namespace GeneratorService.Core.User.Services.Subservices;

public interface IUserProfileNameGeneratorService {
    string GenerateRandomName();
}

public class UserProfileNameGeneratorService : IUserProfileNameGeneratorService {
    private static readonly string[] Adjectives = { "Brave", "Sleepy", "Happy", "Sad", "Angry", "Fast", "Slow", "Smart", "Silly", "Crazy", "Cool", "Mighty", "Fierce", "Gentle", "Wild" };
    private static readonly string[] Animals = { "Tiger", "Bear", "Lion", "Wolf", "Fox", "Eagle", "Shark", "Panther", "Panda", "Koala", "Hawk", "Falcon", "Otter", "Badger", "Owl" };

    public string GenerateRandomName() {
        var random = new Random();
        var adjective = Adjectives[random.Next(Adjectives.Length)];
        var animal = Animals[random.Next(Animals.Length)];
        return $"{adjective}{animal}";
    }
}