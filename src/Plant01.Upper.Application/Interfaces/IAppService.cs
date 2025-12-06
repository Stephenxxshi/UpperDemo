namespace Plant01.Upper.Application.Interfaces;

public interface IAppService
{
    /// <summary>
    /// 上袋机上袋申请
    /// </summary>
    /// <param name="bag"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RequestLoadAsync(string bag, CancellationToken cancellationToken = default);



}
