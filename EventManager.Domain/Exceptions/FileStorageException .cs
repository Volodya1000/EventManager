namespace EventManager.Domain.Exceptions;


public class FileStorageException(string meassage, string ex) : Exception($"File storage exception: {meassage} {ex}");