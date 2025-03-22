using CoreArchV2.Core.Entity.Track;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Interfaces;

namespace CoreArchV2.Services.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<Coordinate> _coordinateRepository;
        public TrackingService(IUnitOfWork uow)
        {
            _uow = uow;
            _coordinateRepository = uow.GetRepository<Coordinate>();
        }

        public async Task InsertCoordinate(string param)
        {
            try
            {
                var splits = param.Split("_").ToList();

                var entities = new List<Coordinate>();
                var dateNow = DateTime.Now;
                foreach (var item in splits)
                {
                    try
                    {
                        var arr = item.Split(",");
                        entities.Add(new Coordinate()
                        {
                            CreatedDate = dateNow,
                            Imei = arr[0],
                            SignalDate = Convert.ToDateTime(arr[1]),
                            Latitude = arr[2],
                            Longitude = arr[3]
                        });
                    }
                    catch (Exception)
                    { }
                }

                await _coordinateRepository.InsertRangeAsync(entities);
                await _uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                //hata maili
            }
        }


        public async Task<List<Coordinate>> GetCoordinate()
        {
            var list = new List<Coordinate>();
            try
            {
                list = await Task.FromResult(_coordinateRepository.GetAll().OrderByDescending(o => o.Id).Take(10).ToList());
            }
            catch (Exception)
            {
                list = null;
            }

            return list;
        }
    }
}
