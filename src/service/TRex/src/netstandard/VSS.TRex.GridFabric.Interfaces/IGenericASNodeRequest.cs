namespace VSS.TRex.GridFabric.Interfaces
{
    public interface IGenericASNodeRequest<TArgument, TResponse>
        where TResponse : class, new()
    {
        /// <summary>
        /// Executes the generic request by instantiating the required ComputeFunc and sending it to 
        /// the compute projection on the grid as defined by the GridName and Role parameters in this request
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>       
        TResponse Execute(TArgument arg);
    }
}
