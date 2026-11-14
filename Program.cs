using System;

// Interface

// Mọi thiết bị đều bật/tắt được
public interface IDevice
{
    void TurnOn();
    void TurnOff();
}

// Chỉ thiết bị có khóa (cửa) mới có thêm interface này
public interface ILockable
{
    void Lock();
    void Unlock();
}

// Các thiết bị

// Đèn thông minh
public class SmartLight : IDevice
{
    private readonly string ten;
    public bool IsOn { get; private set; }

    public SmartLight(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ten khong hop le");
        ten = name;
    }

    public void TurnOn()
    {
        IsOn = true;
        Console.WriteLine(ten + ": ON");
    }

    public void TurnOff()
    {
        IsOn = false;
        Console.WriteLine(ten + ": OFF");
    }
}

// Quạt thông minh
public class SmartFan : IDevice
{
    private readonly string ten;
    public bool IsOn { get; private set; }

    public SmartFan(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ten khong hop le");
        ten = name;
    }

    public void TurnOn()
    {
        IsOn = true;
        Console.WriteLine(ten + ": ON");
    }

    public void TurnOff()
    {
        IsOn = false;
        Console.WriteLine(ten + ": OFF");
    }
}

// Cửa thông minh: bật/tắt và có thêm khóa/mở khóa
public class SmartDoor : IDevice, ILockable
{
    private readonly string ten;
    public bool IsOn { get; private set; }
    public bool IsLocked { get; private set; }

    public SmartDoor(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ten khong hop le");
        ten = name;
    }

    public void TurnOn()
    {
        IsOn = true;
        Console.WriteLine(ten + ": ON");
    }

    public void TurnOff()
    {
        IsOn = false;
        Console.WriteLine(ten + ": OFF");
    }

    public void Lock()
    {
        IsLocked = true;
        Console.WriteLine(ten + ": LOCKED");
    }

    public void Unlock()
    {
        IsLocked = false;
        Console.WriteLine(ten + ": UNLOCKED");
    }
}

// Factory

// Tạo thiết bị theo loại, client không cần gọi constructor của từng lớp
public static class DeviceFactory
{
    public static IDevice Create(string type, string name)
    {
        if (type == null) throw new ArgumentException("Loai thiet bi khong duoc null");

        // Trim và ToLowerInvariant để không phân biệt hoa thường
        return type.Trim().ToLowerInvariant() switch
        {
            "light" => new SmartLight(name),
            "fan" => new SmartFan(name),
            "door" => new SmartDoor(name),
            _ => throw new ArgumentException($"Unsupported device type: {type}")
        };
    }
}

// Singleton

// Cấu hình chung của ngôi nhà, cả chương trình chỉ có một đối tượng
public sealed class HomeConfiguration
{
    // Lazy: chỉ tạo khi dùng lần đầu, và an toàn khi chạy nhiều luồng
    private static readonly Lazy<HomeConfiguration> lazy =
        new Lazy<HomeConfiguration>(() => new HomeConfiguration());

    public static HomeConfiguration Instance => lazy.Value;

    public string HomeName { get; set; } = "My Home";
    public int NightLightLevel { get; set; } = 30;

    // Constructor private: bên ngoài không thể dùng new
    private HomeConfiguration()
    {
    }
}

// Facade

// Giao diện đơn giản cho các chế độ Đi làm / Đi ngủ
public sealed class SmartHomeFacade
{
    private readonly IDevice den;
    private readonly IDevice quat;
    private readonly ILockable cua;

    public SmartHomeFacade(IDevice light, IDevice fan, ILockable door)
    {
        if (light == null) throw new ArgumentNullException(nameof(light));
        if (fan == null) throw new ArgumentNullException(nameof(fan));
        if (door == null) throw new ArgumentNullException(nameof(door));
        den = light;
        quat = fan;
        cua = door;
    }

    // Chế độ đi làm: tắt đèn, tắt quạt, khóa cửa
    public void GoToWork()
    {
        den.TurnOff();
        quat.TurnOff();
        cua.Lock();
    }

    // Chế độ đi ngủ: tắt quạt, khóa cửa, bật đèn ngủ
    public void GoToSleep()
    {
        quat.TurnOff();
        cua.Lock();
        den.TurnOn();

        // Đọc mức sáng từ cấu hình chung
        int muc_sang = HomeConfiguration.Instance.NightLightLevel;
        Console.WriteLine($"Night light level: {muc_sang}%");
    }
}

// Chương trình kiểm thử
public class Program
{
    // In PASS nếu điều kiện đúng, FAIL nếu sai
    private static void Check(bool dieu_kien, string mo_ta)
    {
        Console.WriteLine((dieu_kien ? "  PASS: " : "  FAIL: ") + mo_ta);
    }

    public static void Main()
    {
        // T1: tạo light, fan, door bằng Factory
        Console.WriteLine("T1: Factory tao dung kieu doi tuong");
        IDevice den = DeviceFactory.Create("light", "Living room light");
        IDevice quat = DeviceFactory.Create("fan", "Living room fan");
        IDevice cua = DeviceFactory.Create("door", "Main door");
        Check(den is SmartLight, "light tra ve SmartLight");
        Check(quat is SmartFan, "fan tra ve SmartFan");
        Check(cua is SmartDoor, "door tra ve SmartDoor");

        // Không phân biệt hoa thường, bỏ khoảng trắng hai đầu
        IDevice den_2 = DeviceFactory.Create("  LiGhT ", "Bedroom light");
        Check(den_2 is SmartLight, "\"  LiGhT \" van tra ve SmartLight");

        // T2: loại chưa hỗ trợ
        Console.WriteLine("\nT2: Loai thiet bi khong ho tro");
        bool bat_duoc_loi = false;
        try
        {
            DeviceFactory.Create("camera", "Camera 1");
        }
        catch (ArgumentException loi)
        {
            bat_duoc_loi = true;
            Console.WriteLine("  Thong bao loi: " + loi.Message);
        }
        Check(bat_duoc_loi, "camera nem ArgumentException");

        // T3: Singleton
        Console.WriteLine("\nT3: Singleton");
        HomeConfiguration config_1 = HomeConfiguration.Instance;
        HomeConfiguration config_2 = HomeConfiguration.Instance;
        Check(ReferenceEquals(config_1, config_2), "hai lan lay Instance cung mot doi tuong");
        config_1.HomeName = "Nha cua Mai";
        Check(config_2.HomeName == "Nha cua Mai", "doi HomeName qua config_1, config_2 cung thay doi");

        // Tạo facade (ép kiểu cửa về SmartDoor vì cần ILockable)
        var khoa_cua = (SmartDoor)cua;
        var nha_thong_minh = new SmartHomeFacade(den, quat, khoa_cua);

        // T4: chế độ đi làm
        Console.WriteLine("\nT4: GoToWork");
        den.TurnOn();
        quat.TurnOn();
        khoa_cua.Unlock();
        nha_thong_minh.GoToWork();
        Check(!((SmartLight)den).IsOn, "den da tat");
        Check(!((SmartFan)quat).IsOn, "quat da tat");
        Check(khoa_cua.IsLocked, "cua da khoa");

        // T5: chế độ đi ngủ (đổi mức sáng để thấy facade đọc từ cấu hình)
        Console.WriteLine("\nT5: GoToSleep");
        HomeConfiguration.Instance.NightLightLevel = 40;
        quat.TurnOn();
        khoa_cua.Unlock();
        nha_thong_minh.GoToSleep();
        Check(!((SmartFan)quat).IsOn, "quat da tat");
        Check(khoa_cua.IsLocked, "cua da khoa");
        Check(((SmartLight)den).IsOn, "den ngu da bat");
        Console.WriteLine("  (Dong tren phai hien: Night light level: 40%)");

        // T6: client vẫn điều khiển riêng từng thiết bị được
        Console.WriteLine("\nT6: Dieu khien rieng tung thiet bi");
        den.TurnOff();
        Check(!((SmartLight)den).IsOn, "tat rieng den, khong can qua facade");
    }
}