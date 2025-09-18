namespace App.Core.Models.Sell;

public class ProductDimensions
{
    public double WidthMm { get; set; }
    public double HeightMm { get; set; }
    public double DepthMm { get; set; }
    public double MassKg { get; set; }
    
    public double VolumeCubicMm => WidthMm * HeightMm * DepthMm;
    public double VolumeLiters => VolumeCubicMm / 1_000_000.0;
    
    public double VolumetricWeightKg(int coefficient = 4000)
        => VolumeCubicMm / coefficient;

}