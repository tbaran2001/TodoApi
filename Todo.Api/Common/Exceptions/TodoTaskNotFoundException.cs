namespace Todo.Api.Common.Exceptions;

public class TodoTaskNotFoundException(Guid id) : NotFoundException("TodoTask", id);