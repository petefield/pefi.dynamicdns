using OneOf;
using OneOf.Types;

namespace pefi.dynamicdns.Infrastructure;


[GenerateOneOf]
public partial class Result : OneOfBase<Success, Error> { }
