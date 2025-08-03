using System;
using System.Collections.Generic;

namespace HotelReservationApp.TempModels;

public partial class RoomType
{
    public int RoomTypeId { get; set; }

    public string TypeName { get; set; } = null!;
}
