using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PictureProcessing
{
    // 定义转换方法
    enum Transform { Distortion, Warp, TPS, None };

    // 定义插值方法
    enum Interpolation { NearestNeighbor, BiLinear, BiCubic, None };

    // 图片参数类
    class ImageParameter
    {
        private readonly ImageIO _sourceImage;
        private readonly ImageIO _faceImage;
        private readonly string _sourcePath;
        private readonly string _facePath;

        public string SourcePath => _sourcePath;

        public string FacePath => _facePath;

        internal ImageIO FaceImage => _faceImage;

        internal ImageIO SourceImage => _sourceImage;

        public ImageParameter(ImageIO sourceImage, ImageIO faceImage, string sourcePath, string facePath)
        {
            _sourceImage = sourceImage;
            _faceImage = faceImage;
            _sourcePath = sourcePath;
            _facePath = facePath;
        }
    }

    // 处理选项类
    class ProcessOption
    {
        private readonly string _transformType;
        private readonly Transform _transform;
        private readonly string _interpolationType;
        private readonly Interpolation _interpolation;

        public string TransformType => _transformType;

        public string InterpolationType => _interpolationType;

        internal Transform Transform => _transform;

        internal Interpolation Interpolation => _interpolation;

        public ProcessOption(Transform transform, Interpolation interpolation)
        {
            _transform = transform;
            _interpolation = interpolation;
            switch (transform)
            {
                case Transform.Distortion:
                    {
                        _transformType = "Transform.Distortion";
                        break;
                    }
                case Transform.Warp:
                    {
                        _transformType = "Transform.Warp";
                        break;
                    }
                case Transform.TPS:
                    {
                        _transformType = "Transform.TPS";
                        break;
                    }
                case Transform.None:
                    {
                        _transformType = "Transform.None";
                        break;
                    }
                default:
                    {
                        _transformType = null;
                        break;
                    }
            }
            switch (interpolation)
            {
                case Interpolation.NearestNeighbor:
                    {
                        _interpolationType = "Interpolation.NearestNeighbor";
                        break;
                    }
                case Interpolation.BiLinear:
                    {
                        _interpolationType = "Interpolation.BiLinear";
                        break;
                    }
                case Interpolation.BiCubic:
                    {
                        _interpolationType = "Interpolation.BiCubic";
                        break;
                    }
                case Interpolation.None:
                    {
                        _interpolationType = "Interpolation.None";
                        break;
                    }
                default:
                    {
                        _interpolationType = null;
                        break;
                    }
            }
        }
    }

    // 处理结果类
    class ProcessResult
    {
        private readonly ImageIO _processedImage;
        private readonly ImageIO _originalMarkedImage;
        private readonly ImageIO _faceMarkedImage;

        internal ImageIO ProcessedImage => _processedImage;

        internal ImageIO OriginalMarkedImage => _originalMarkedImage;

        internal ImageIO FaceMarkedImage => _faceMarkedImage;

        public ProcessResult(ImageIO processedImage, ImageIO originalMarkedImage, ImageIO faceMarkedImage)
        {
            _processedImage = processedImage;
            _originalMarkedImage = originalMarkedImage;
            _faceMarkedImage = faceMarkedImage;
        }
    };

    // 旋转扭曲参数类
    class DistortionParameter
    {
        private readonly double _maxDistortionDegree;
        private readonly double _distortionRadius;
        private readonly int _distortionCenter_x;
        private readonly int _distortionCenter_y;
        private readonly double _factor_x;
        private readonly double _factor_y;

        public double MaxDistortionDegree => _maxDistortionDegree;

        public double DistortionRadius => _distortionRadius;

        public int DistortionCenter_x => _distortionCenter_x;

        public int DistortionCenter_y => _distortionCenter_y;

        public double Factor_x => _factor_x;

        public double Factor_y => _factor_y;

        public DistortionParameter(double maxDistortionDegree, double distortionRadius, int distortionCenter_x, int distortionCenter_y, double factor_x = 1, double factor_y = 1)
        {
            _maxDistortionDegree = maxDistortionDegree;
            _distortionRadius = distortionRadius;
            _distortionCenter_x = distortionCenter_x;
            _distortionCenter_y = distortionCenter_y;
            _factor_x = factor_x;
            _factor_y = factor_y;
        }
    };

    // 畸变矫正参数类
    class WarpParameter
    {
        private readonly double _warpFactor;
        private readonly int _warpCenter_x;
        private readonly int _warpCenter_y;
        private readonly double _factor_x;
        private readonly double _factor_y;
        private readonly double _warpScale;

        public double WarpFactor => _warpFactor;

        public int WarpCenter_x => _warpCenter_x;

        public int WarpCenter_y => _warpCenter_y;

        public double Factor_x => _factor_x;

        public double Factor_y => _factor_y;

        public double WarpScale => _warpScale;

        public WarpParameter(double warpScale, double warpFactor, int warpCenter_x, int warpCenter_y, double factor_x = 1, double factor_y = 1)
        {
            _warpScale = warpScale;
            _warpFactor = warpFactor;
            _warpCenter_x = warpCenter_x;
            _warpCenter_y = warpCenter_y;
            _factor_x = factor_x;
            _factor_y = factor_y;
        }
    };

    // 图像位置类
    class Poseition
    {
        readonly double _x;
        readonly double _y;

        public double X => _x;

        public double Y => _y;

        public Poseition(double x, double y)
        {
            this._x = x;
            this._y = y;
        }
    }

    // 插值参数类
    class InterpolationParameter
    {
        readonly int _targetWidth;
        readonly int _targetHeight;
        readonly double _bicubicFactor;

        public int TargetWidth => _targetWidth;

        public int TargetHeight => _targetHeight;

        public double BicubicFactor => _bicubicFactor;

        public InterpolationParameter(int targetWidth, int targetHeight, double bicubicFactor)
        {
            _targetWidth = targetWidth;
            _targetHeight = targetHeight;
            _bicubicFactor = bicubicFactor;
        }
    }

    // 声明：MainWindow 部分功能实现思路参考了互联网相关资料，具体实现为笔者自行完成，相关部分已用 * 
    public partial class MainWindow : Window
    {
        // 变量存储区
        private readonly ComboBoxItem TransformDistortion;
        private readonly ComboBoxItem TransformWarp;
        private readonly ComboBoxItem TransformTPS;
        private readonly ComboBoxItem TransformNone;
        private readonly ComboBoxItem InterpolationNearestNeighbor;
        private readonly ComboBoxItem InterpolationBiLinear;
        private readonly ComboBoxItem InterpolationBiCubic;
        private readonly ComboBoxItem InterpolationNone;
        private readonly ComboBoxItem[] TPSSourceSelection;
        private readonly ComboBoxItem[] TPSFaceSelection;
        private string _originalPicturePath;
        private string _processedPicturePath;
        private string _facePicturePath;
        private ImageIO _originalPictureData;
        private ImageIO _processedPictureData;
        private ImageIO _facePictureData;
        private ImageIO _originalMarkedPicture;
        private ImageIO _faceMarkedPicture;
        private double _maxDegree;
        private double _radius;
        private int _centerX;
        private int _centerY;
        private int _targetWidth;
        private int _targetHeight;
        private double _warpFactor;
        private double _bicubicFactor = -0.5;
        private Transform _transformMethod;
        private Interpolation _interpolationMethod;

        // 数据接口
        public string OriginalPicturePath { get => _originalPicturePath; set => _originalPicturePath = value; }
        public double MaxDegree { get => _maxDegree; set => _maxDegree = value; }
        public string ProcessedPicturePath { get => _processedPicturePath; set => _processedPicturePath = value; }
        internal ImageIO OriginalPictureData { get => _originalPictureData; set => _originalPictureData = value; }
        internal ImageIO ProcessedPictureData { get => _processedPictureData; set => _processedPictureData = value; }
        public double Radius { get => _radius; set => _radius = value; }
        public int CenterX { get => _centerX; set => _centerX = value; }
        public int CenterY { get => _centerY; set => _centerY = value; }
        public int TargetWidth { get => _targetWidth; set => _targetWidth = value; }
        public int TargetHeight { get => _targetHeight; set => _targetHeight = value; }
        public double WarpFactor { get => _warpFactor; set => _warpFactor = value; }
        public double BicubicFactor { get => _bicubicFactor; set => _bicubicFactor = value; }
        internal Transform TransformMethod { get => _transformMethod; set => _transformMethod = value; }
        internal Interpolation InterpolationMethod { get => _interpolationMethod; set => _interpolationMethod = value; }
        public string FacePicturePath { get => _facePicturePath; set => _facePicturePath = value; }
        internal ImageIO FacePictureData { get => _facePictureData; set => _facePictureData = value; }
        internal ImageIO OriginalMarkedPicture { get => _originalMarkedPicture; set => _originalMarkedPicture = value; }
        internal ImageIO FaceMarkedPicture { get => _faceMarkedPicture; set => _faceMarkedPicture = value; }

        // 构造函数
        public MainWindow()
        {
            InitializeComponent();
            this.Debug.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " info: program initialized.";
            TransformDistortion = new ComboBoxItem
            {
                Content = "Distortion"
            };
            this.TransformType.Items.Add(TransformDistortion);
            TransformWarp = new ComboBoxItem
            {
                Content = "Warp"
            };
            this.TransformType.Items.Add(TransformWarp);
            TransformTPS = new ComboBoxItem
            {
                Content = "TPS"
            };
            this.TransformType.Items.Add(TransformTPS);
            TransformNone = new ComboBoxItem
            {
                Content = "None"
            };
            this.TransformType.Items.Add(TransformNone);
            InterpolationNearestNeighbor = new ComboBoxItem
            {
                Content = "NearestNeighbor"
            };
            this.InterpolationType.Items.Add(InterpolationNearestNeighbor);
            InterpolationBiLinear = new ComboBoxItem
            {
                Content = "BiLinear"
            };
            this.InterpolationType.Items.Add(InterpolationBiLinear);
            InterpolationBiCubic = new ComboBoxItem
            {
                Content = "BiCubic"
            };
            this.InterpolationType.Items.Add(InterpolationBiCubic);
            InterpolationNone = new ComboBoxItem
            {
                Content = "None"
            };
            this.InterpolationType.Items.Add(InterpolationNone);
            this.TPSSourceSelection = new ComboBoxItem[9];
            for (int i = 0; i < 9; i++) 
            {
                this.TPSSourceSelection[i] = new ComboBoxItem();
                this.TPSSourceSelection[i].Content = (i + 1).ToString();
                this.SourceImageSelection.Items.Add(this.TPSSourceSelection[i]);
            }
            this.TPSFaceSelection = new ComboBoxItem[9];
            for (int i = 0; i < 9; i++)
            {
                this.TPSFaceSelection[i] = new ComboBoxItem();
                this.TPSFaceSelection[i].Content = (i + 1).ToString();
                this.FaceImageSelection.Items.Add(this.TPSFaceSelection[i]);
            }
            this.HideDistortionOption();
            this.HideWarpOption();
            this.HideInterpolationOption();
            this.HideTPSOption();
        }

        // 根据 id 获取库中 TPS 数据
        private ImageIO TPSSelectionImage(string id)
        {
            switch(id)
            {
                case "1":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\1.jpg");
                    }
                case "2":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\2.jpg");
                    }
                case "3":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\3.jpg");
                    }
                case "4":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\4.jpg");
                    }
                case "5":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\5.jpg");
                    }
                case "6":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\6.jpg");
                    }
                case "7":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\7.jpg");
                    }
                case "8":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\8.jpg");
                    }
                case "9":
                    {
                        return new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\9.jpg");
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        // * 窗体移动函数
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // 获取鼠标相对标题栏位置  
            Point position = e.GetPosition(this);

            // 如果鼠标位置在标题栏内，允许拖动  
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (position.X >= 0 && position.X < this.ActualWidth && position.Y >= 0 && position.Y < this.ActualHeight)
                {
                    this.DragMove();
                }
            }
        }

        // 不同 UI Group 的显示与隐藏
        private void ShowNearestNeighborOption()
        {
            this.TargetHeightLabel.Visibility = Visibility.Visible;
            this.TargetHeightValue.Visibility = Visibility.Visible;
            this.TargetWidthLabel.Visibility = Visibility.Visible;
            this.TargetWidthValue.Visibility = Visibility.Visible;
            this.TargetHeightValue.Text = this.OriginalPictureData.Height.ToString();
            this.TargetWidthValue.Text = this.OriginalPictureData.Width.ToString();
        }
        private void HideNearestNeighborOption()
        {
            this.TargetHeightLabel.Visibility = Visibility.Hidden;
            this.TargetHeightValue.Visibility = Visibility.Hidden;
            this.TargetWidthLabel.Visibility = Visibility.Hidden;
            this.TargetWidthValue.Visibility = Visibility.Hidden;
        }

        private void ShowBiLinearOption()
        {
            this.TargetHeightLabel.Visibility = Visibility.Visible;
            this.TargetHeightValue.Visibility = Visibility.Visible;
            this.TargetWidthLabel.Visibility = Visibility.Visible;
            this.TargetWidthValue.Visibility = Visibility.Visible;
            this.TargetHeightValue.Text = this.OriginalPictureData.Height.ToString();
            this.TargetWidthValue.Text = this.OriginalPictureData.Width.ToString();
        }
        private void HideBiLinearOption()
        {
            this.TargetHeightLabel.Visibility = Visibility.Hidden;
            this.TargetHeightValue.Visibility = Visibility.Hidden;
            this.TargetWidthLabel.Visibility = Visibility.Hidden;
            this.TargetWidthValue.Visibility = Visibility.Hidden;
        }

        private void ShowBiCubicOption()
        {
            this.TargetHeightLabel.Visibility = Visibility.Visible;
            this.TargetHeightValue.Visibility = Visibility.Visible;
            this.TargetWidthLabel.Visibility = Visibility.Visible;
            this.TargetWidthValue.Visibility = Visibility.Visible;
            this.BicubicFactorLabel.Visibility = Visibility.Visible;
            this.BicubicFactorValue.Visibility = Visibility.Visible;
            this.TargetHeightValue.Text = this.OriginalPictureData.Height.ToString();
            this.TargetWidthValue.Text = this.OriginalPictureData.Width.ToString();
            this.BicubicFactorValue.Text = "-0.5";
        }
        private void HideBiCubicOption()
        {
            this.TargetHeightLabel.Visibility = Visibility.Hidden;
            this.TargetHeightValue.Visibility = Visibility.Hidden;
            this.TargetWidthLabel.Visibility = Visibility.Hidden;
            this.TargetWidthValue.Visibility = Visibility.Hidden;
            this.BicubicFactorLabel.Visibility = Visibility.Hidden;
            this.BicubicFactorValue.Visibility = Visibility.Hidden;
        }

        private void ShowDistortionOption()
        {
            this.MaxDegreeLabel.Visibility = Visibility.Visible;
            this.MaxDegreeValue.Visibility = Visibility.Visible;
            this.RadiusLabel.Visibility = Visibility.Visible;
            this.RadiusValue.Visibility = Visibility.Visible;
            this.CenterXLabel.Visibility = Visibility.Visible;
            this.CenterXValue.Visibility = Visibility.Visible;
            this.CenterYLabel.Visibility = Visibility.Visible;
            this.CenterYValue.Visibility = Visibility.Visible;
            this.InterpolationOptionLabel.Visibility = Visibility.Visible;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Visible;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.Process.Visibility = Visibility.Visible;
            this.OriginalImageLabel.Visibility = Visibility.Visible;
            this.SelectPicture.Visibility = Visibility.Visible;
            this.ProcessedImageLabel.Visibility = Visibility.Visible;
            this.SavePicture.Visibility = Visibility.Visible;
            this.OriginalPicturePath = @"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\THU.jpg";
            this.OriginalPictureData = new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\THU.jpg");
            this.OriginalPictureData.LockBits();
            this.OriginalPictureData.UnlockBits();
            this.OriginalPicture.Source = new BitmapImage(new Uri(this.OriginalPicturePath));
            this.AddInfo("open original picture " + this.OriginalPicturePath);
            this.AddInfo("Width: " + this.OriginalPictureData.Width.ToString());
            this.AddInfo("Height: " + this.OriginalPictureData.Height.ToString());
            this.ProcessedPictureData = null;
            this.ProcessedPicturePath = null;
            this.FacePictureData = null;
            this.FacePicturePath = null;
            this.ProcessedPicture.Source = null;
            this.MaxDegreeValue.Text = "1";
            this.RadiusValue.Text = "255";
            this.CenterXValue.Text = "255";
            this.CenterYValue.Text = "255";
        }
        private void HideDistortionOption()
        {
            this.MaxDegreeLabel.Visibility = Visibility.Hidden;
            this.MaxDegreeValue.Visibility = Visibility.Hidden;
            this.RadiusLabel.Visibility = Visibility.Hidden;
            this.RadiusValue.Visibility = Visibility.Hidden;
            this.CenterXLabel.Visibility = Visibility.Hidden;
            this.CenterXValue.Visibility = Visibility.Hidden;
            this.CenterYLabel.Visibility = Visibility.Hidden;
            this.CenterYValue.Visibility = Visibility.Hidden;
            this.InterpolationOptionLabel.Visibility = Visibility.Hidden;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Hidden;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.Process.Visibility = Visibility.Hidden;
            this.OriginalImageLabel.Visibility = Visibility.Hidden;
            this.SelectPicture.Visibility = Visibility.Hidden;
            this.ProcessedImageLabel.Visibility = Visibility.Hidden;
            this.SavePicture.Visibility = Visibility.Hidden;
        }

        private void ShowWarpOption()
        {
            this.WarpFactorLabel.Visibility = Visibility.Visible;
            this.WarpFactorValue.Visibility = Visibility.Visible;
            this.CenterXLabel.Visibility = Visibility.Visible;
            this.CenterXValue.Visibility = Visibility.Visible;
            this.CenterYLabel.Visibility = Visibility.Visible;
            this.CenterYValue.Visibility = Visibility.Visible;
            this.InterpolationOptionLabel.Visibility = Visibility.Visible;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Visible;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.Process.Visibility = Visibility.Visible;
            this.OriginalImageLabel.Visibility = Visibility.Visible;
            this.SelectPicture.Visibility = Visibility.Visible;
            this.ProcessedImageLabel.Visibility = Visibility.Visible;
            this.SavePicture.Visibility = Visibility.Visible;
            this.OriginalPicturePath = @"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\THU.jpg";
            this.OriginalPictureData = new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\THU.jpg");
            this.OriginalPictureData.LockBits();
            this.OriginalPictureData.UnlockBits();
            this.OriginalPicture.Source = new BitmapImage(new Uri(this.OriginalPicturePath));
            this.AddInfo("open original picture " + this.OriginalPicturePath);
            this.AddInfo("Width: " + this.OriginalPictureData.Width.ToString());
            this.AddInfo("Height: " + this.OriginalPictureData.Height.ToString());
            this.ProcessedPictureData = null;
            this.ProcessedPicturePath = null;
            this.FacePictureData = null;
            this.FacePicturePath = null;
            this.ProcessedPicture.Source = null;
            this.WarpFactorValue.Text = "0.5";
            this.CenterXValue.Text = "255";
            this.CenterYValue.Text = "255";
        }
        private void HideWarpOption()
        {
            this.WarpFactorLabel.Visibility = Visibility.Hidden;
            this.WarpFactorValue.Visibility = Visibility.Hidden;
            this.CenterXLabel.Visibility = Visibility.Hidden;
            this.CenterXValue.Visibility = Visibility.Hidden;
            this.CenterYLabel.Visibility = Visibility.Hidden;
            this.CenterYValue.Visibility = Visibility.Hidden;
            this.InterpolationOptionLabel.Visibility = Visibility.Hidden;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Hidden;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.Process.Visibility = Visibility.Hidden;
            this.OriginalImageLabel.Visibility = Visibility.Hidden;
            this.SelectPicture.Visibility = Visibility.Hidden;
            this.ProcessedImageLabel.Visibility = Visibility.Hidden;
            this.SavePicture.Visibility = Visibility.Hidden;
        }

        private void ShowTPSOption()
        {
            this.SourceImageSelectionLable.Visibility = Visibility.Visible;
            this.SourceImageSelection.SelectedIndex = 7;
            this.SourceImageSelection.Visibility = Visibility.Visible;
            this.FaceImageSelectionLable.Visibility = Visibility.Visible;
            this.FaceImageSelection.SelectedIndex = 5;
            this.FaceImageSelection.Visibility = Visibility.Visible;
            this.SourceImageLabel.Visibility = Visibility.Visible;
            this.FaceImageLabel.Visibility = Visibility.Visible;
            this.InterpolationOptionLabel.Visibility = Visibility.Visible;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Visible;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.OriginalPictureData = null;
            this.OriginalPicturePath = null;
            this.ProcessedPictureData = null;
            this.ProcessedPicturePath = null;
            this.FacePictureData = null;
            this.FacePicturePath = null;
            this.OriginalPicture.Source = null;
            this.ProcessedPicture.Source = null;
            this.Process.Visibility = Visibility.Visible;
            this.OriginalPictureData = this.TPSSelectionImage("8");
            this.OriginalPictureData.LockBits();
            this.OriginalPictureData.UnlockBits();
            this.OriginalPicturePath = this.OriginalPictureData.Path;
            this.OriginalPicture.Source = new BitmapImage(new Uri(this.OriginalPicturePath));
            this.AddInfo("open source picture " + this.OriginalPicturePath);
            this.AddInfo("Width: " + this.OriginalPictureData.Width.ToString());
            this.AddInfo("Height: " + this.OriginalPictureData.Height.ToString());
            this.FacePictureData = this.TPSSelectionImage("6");
            this.FacePictureData.LockBits();
            this.FacePictureData.UnlockBits();
            this.FacePicturePath = this.FacePictureData.Path;
            this.ProcessedPicture.Source = new BitmapImage(new Uri(this.FacePicturePath));
            this.AddInfo("open face picture " + this.FacePicturePath);
            this.AddInfo("Width: " + this.FacePictureData.Width.ToString());
            this.AddInfo("Height: " + this.FacePictureData.Height.ToString());
        }
        private void HideTPSOption()
        {
            this.SourceImageSelectionLable.Visibility = Visibility.Hidden;
            this.SourceImageSelection.SelectedIndex = -1;
            this.SourceImageSelection.Visibility = Visibility.Hidden;
            this.FaceImageSelectionLable.Visibility = Visibility.Hidden;
            this.FaceImageSelection.SelectedIndex = -1;
            this.FaceImageSelection.Visibility = Visibility.Hidden;
            this.SourceImageLabel.Visibility = Visibility.Hidden;
            this.FaceImageLabel.Visibility = Visibility.Hidden;
            this.InterpolationOptionLabel.Visibility = Visibility.Hidden;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Hidden;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.Process.Visibility = Visibility.Hidden;
            this.ShowSourceMarked.Visibility = Visibility.Hidden;
            this.ShowFaceMarked.Visibility = Visibility.Hidden;
            this.SaveTPS.Visibility = Visibility.Hidden;
            this.ShowTPS.Visibility = Visibility.Hidden;
        }

        private void ShowInterpolationOption()
        {
            this.InterpolationOptionLabel.Visibility = Visibility.Visible;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Visible;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.Process.Visibility = Visibility.Visible;
            this.OriginalImageLabel.Visibility = Visibility.Visible;
            this.SelectPicture.Visibility = Visibility.Visible;
            this.ProcessedImageLabel.Visibility = Visibility.Visible;
            this.SavePicture.Visibility = Visibility.Visible;
            this.OriginalPicturePath = @"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\THU.jpg";
            this.OriginalPictureData = new ImageIO(@"C:\Users\Antonio\Documents\Visual Studio Projects\PictureProcessing\PictureProcessing\src\Image\THU.jpg");
            this.OriginalPictureData.LockBits();
            this.OriginalPictureData.UnlockBits();
            this.OriginalPicture.Source = new BitmapImage(new Uri(this.OriginalPicturePath));
            this.AddInfo("open original picture " + this.OriginalPicturePath);
            this.AddInfo("Width: " + this.OriginalPictureData.Width.ToString());
            this.AddInfo("Height: " + this.OriginalPictureData.Height.ToString());
            this.ProcessedPictureData = null;
            this.ProcessedPicturePath = null;
            this.FacePictureData = null;
            this.FacePicturePath = null;
            this.ProcessedPicture.Source = null;
        }
        private void HideInterpolationOption()
        {
            this.InterpolationOptionLabel.Visibility = Visibility.Hidden;
            this.InterpolationType.SelectedIndex = -1;
            this.InterpolationType.Visibility = Visibility.Hidden;
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            this.Process.Visibility = Visibility.Hidden;
            this.OriginalImageLabel.Visibility = Visibility.Hidden;
            this.SelectPicture.Visibility = Visibility.Hidden;
            this.ProcessedImageLabel.Visibility = Visibility.Hidden;
            this.SavePicture.Visibility = Visibility.Hidden;
        }

        // 预加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        // 最小化
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 退出
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 处理图像
        private void Process_Click(object sender, RoutedEventArgs e)
        {
            if((this.OriginalPicture==null)||(this.OriginalPicturePath==null))
            {
                Error errorBox = new Error();
                errorBox.ShowDialog();
                this.AddError("Please select picture first!");
            }
            else if(!this.ParameterCheck())
            {
                Error errorBox = new Error();
                errorBox.ShowDialog();
            }
            else
            {
                this.AddInfo("Process start.");
                switch (this.TransformMethod)
                {
                    case Transform.Distortion: this.AddInfo("Transform mode: Distortion");break;
                    case Transform.Warp:this.AddInfo("Transform mode: Warp");break;
                    case Transform.TPS: this.AddInfo("Transform mode: TPS"); break;
                    case Transform.None: this.AddInfo("Transform mode: None"); break;
                }
                switch(this.InterpolationMethod)
                {
                    case Interpolation.NearestNeighbor: this.AddInfo("Interpolation mode: NearestNeighbor");break;
                    case Interpolation.BiLinear: this.AddInfo("Interpolation mode: BiLinear"); break;
                    case Interpolation.BiCubic: this.AddInfo("Interpolation mode: BiCubic"); break;
                    case Interpolation.None: this.AddInfo("Interpolation mode: None"); break;
                }
                if (this.InterpolationType.SelectedItem == this.InterpolationNone)
                {
                    this.TargetHeight = this.OriginalPictureData.Height;
                    this.TargetWidth = this.OriginalPictureData.Width;
                }
                if((this.TargetHeight<=0)||(this.TargetWidth<=0))
                {
                    this.TargetHeight = this.OriginalPictureData.Height;
                    this.TargetWidth = this.OriginalPictureData.Width;
                    this.AddError("Target Height and Target Width are wrong, using Original Scale instead!");
                }
                this.AddInfo("Target Width: " + this.TargetWidth);
                this.AddInfo("Target Height: " + this.TargetHeight);
                if (this.TransformType.SelectedItem == this.TransformDistortion)
                {
                    ImageProcess imageProcess = new ImageProcess(this.OriginalPictureData, this.MaxDegree, this.Radius, this.CenterX, this.CenterY, this.TargetWidth, this.TargetHeight, this.InterpolationMethod, this.BicubicFactor);
                    this.ProcessedPictureData = imageProcess.ProcessResult.ProcessedImage;
                    this.ProcessedPicture.Source = this.BitmapToBitmapImage(this.ProcessedPictureData.Bitmap);
                }

                if (this.TransformType.SelectedItem == this.TransformWarp)
                {
                    ImageProcess imageProcess = new ImageProcess(this.OriginalPictureData, this.WarpFactor, this.CenterX, this.CenterY, this.TargetWidth, this.TargetHeight, this.InterpolationMethod, this.BicubicFactor);
                    this.ProcessedPictureData = imageProcess.ProcessResult.ProcessedImage;
                    this.ProcessedPicture.Source = this.BitmapToBitmapImage(this.ProcessedPictureData.Bitmap);
                }

                if (this.TransformType.SelectedItem == this.TransformTPS)
                {
                    ImageProcess imageProcess = new ImageProcess(this.OriginalPictureData, this.FacePictureData, this.TargetWidth, this.TargetHeight, this.InterpolationMethod, this.BicubicFactor);
                    this.ProcessedPictureData = imageProcess.ProcessResult.ProcessedImage;
                    this.OriginalMarkedPicture = imageProcess.ProcessResult.OriginalMarkedImage;
                    this.FaceMarkedPicture = imageProcess.ProcessResult.FaceMarkedImage;
                    //this.ProcessedPicture.Source = this.BitmapToBitmapImage(this.ProcessedPictureData.Bitmap);
                    this.ShowSourceMarked.Visibility = Visibility.Visible;
                    this.ShowFaceMarked.Visibility = Visibility.Visible;
                    this.SaveTPS.Visibility = Visibility.Visible;
                    this.ShowTPS.Visibility = Visibility.Visible;
                }

                if (this.TransformType.SelectedItem == this.TransformNone)
                {
                    ImageProcess imageProcess = new ImageProcess(this.OriginalPictureData, this.TargetWidth, this.TargetHeight, this.InterpolationMethod, this.BicubicFactor);
                    this.ProcessedPictureData = imageProcess.ProcessResult.ProcessedImage;
                    this.ProcessedPicture.Source = this.BitmapToBitmapImage(this.ProcessedPictureData.Bitmap);
                }

                this.Debug.ScrollToEnd();
                this.AddInfo("Process done.");
            }
        }

        // 参数检查
        private bool ParameterCheck()
        {
            if (this.TransformType.SelectedItem == null) 
            {
                this.AddError("please select transform type.");
                return false;
            }
            if (this.InterpolationType.SelectedItem == null)
            {
                this.AddError("please select interpolation type.");
                return false;
            }
            if(this.TransformType.SelectedItem==this.TransformDistortion)
            {
                this.TransformMethod = Transform.Distortion;
                if(double.TryParse(this.MaxDegreeValue.Text,out this._maxDegree))
                {
                    this.AddInfo("MaxDegree is set, its value is " + this.MaxDegreeValue.Text);
                }
                else
                {
                    this.AddError("MaxDegree could not be set, please check.");
                    return false;
                }

                if (double.TryParse(this.RadiusValue.Text, out this._radius))
                {
                    this.AddInfo("Radius is set, its value is " + this.RadiusValue.Text);
                }
                else
                {
                    this.AddError("Radius could not be set, please check.");
                    return false;
                }

                if (int.TryParse(this.CenterXValue.Text, out this._centerX))
                {
                    this.AddInfo("CenterX is set, its value is " + this.CenterXValue.Text);
                }
                else
                {
                    this.AddError("CenterX could not be set, please check.");
                    return false;
                }

                if (int.TryParse(this.CenterYValue.Text, out this._centerY))
                {
                    this.AddInfo("CenterY is set, its value is " + this.CenterYValue.Text);
                }
                else
                {
                    this.AddError("CenterY could not be set, please check.");
                    return false;
                }
            }

            if(this.TransformType.SelectedItem==this.TransformWarp)
            {
                this.TransformMethod = Transform.Warp;
                if (double.TryParse(this.WarpFactorValue.Text, out this._warpFactor))
                {
                    this.AddInfo("WarpFactor is set, its value is " + this.WarpFactorValue.Text);
                }
                else
                {
                    this.AddError("WarpFactor could not be set, please check.");
                    return false;
                }

                if (int.TryParse(this.CenterXValue.Text, out this._centerX))
                {
                    this.AddInfo("CenterX is set, its value is " + this.CenterXValue.Text);
                }
                else
                {
                    this.AddError("CenterX could not be set, please check.");
                    return false;
                }

                if (int.TryParse(this.CenterYValue.Text, out this._centerY))
                {
                    this.AddInfo("CenterY is set, its value is " + this.CenterYValue.Text);
                }
                else
                {
                    this.AddError("CenterY could not be set, please check.");
                    return false;
                }
            }

            if(this.TransformType.SelectedItem==this.TransformTPS)
            {
                this.TransformMethod = Transform.TPS;
            }

            if(this.TransformType.SelectedItem==this.TransformNone)
            {
                this.TransformMethod = Transform.None;
            }

            if(this.InterpolationType.SelectedItem==this.InterpolationNearestNeighbor)
            {
                this.InterpolationMethod = Interpolation.NearestNeighbor;
                if (int.TryParse(this.TargetWidthValue.Text, out this._targetWidth))
                {
                    this.AddInfo("TargetWidth is set, its value is " + this.TargetWidthValue.Text);
                }
                else
                {
                    this.AddError("TargetWidth could not be set, please check.");
                    return false;
                }

                if (int.TryParse(this.TargetHeightValue.Text, out this._targetHeight))
                {
                    this.AddInfo("TargetHeight is set, its value is " + this.TargetHeightValue.Text);
                }
                else
                {
                    this.AddError("TargetHeight could not be set, please check.");
                    return false;
                }
            }

            if (this.InterpolationType.SelectedItem ==this.InterpolationBiLinear)
            {
                this.InterpolationMethod = Interpolation.BiLinear;
                if (int.TryParse(this.TargetWidthValue.Text, out this._targetWidth))
                {
                    this.AddInfo("TargetWidth is set, its value is " + this.TargetWidthValue.Text);
                }
                else
                {
                    this.AddError("TargetWidth could not be set, please check.");
                    return false;
                }

                if (int.TryParse(this.TargetHeightValue.Text, out this._targetHeight))
                {
                    this.AddInfo("TargetHeight is set, its value is " + this.TargetHeightValue.Text);
                }
                else
                {
                    this.AddError("TargetHeight could not be set, please check.");
                    return false;
                }
            }

            if (this.InterpolationType.SelectedItem ==this.InterpolationBiCubic)
            {
                this.InterpolationMethod = Interpolation.BiCubic;
                if (int.TryParse(this.TargetWidthValue.Text, out this._targetWidth))
                {
                    this.AddInfo("TargetWidth is set, its value is " + this.TargetWidthValue.Text);
                }
                else
                {
                    this.AddError("TargetWidth could not be set, please check.");
                    return false;
                }

                if (int.TryParse(this.TargetHeightValue.Text, out this._targetHeight))
                {
                    this.AddInfo("TargetHeight is set, its value is " + this.TargetHeightValue.Text);
                }
                else
                {
                    this.AddError("TargetHeight could not be set, please check.");
                    return false;
                }

                if(double.TryParse(this.BicubicFactorValue.Text, out this._bicubicFactor))
                {
                    this.AddInfo("BicubicFactor is set, its value is " + this.BicubicFactorValue.Text);
                }
                else
                {
                    this.AddError("BicubicFactor could not be set, please check.");
                    return false;
                }
            }

            if (this.InterpolationType.SelectedItem ==this.InterpolationNone)
            {
                this.InterpolationMethod = Interpolation.None;
            }
            return true;
        }

        // 日志记录
        private void AddInfo(string info)
        {
            this.Debug.Text += System.Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " info: " + info;
            this.Debug.ScrollToEnd();
        }
        private void AddError(string error)
        {
            this.Debug.Text += System.Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " error: " + error;
            this.Debug.ScrollToEnd();
        }

        // 选择图片
        private void SelectPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open original picture...";
            openFileDialog.Filter = "Picture Files(*.jgp; *.png; *.jpeg; *.bmp) | *.jgp; *.png; *.jpeg; *.bmp; | All Files(*.*) | *.* ";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "Picture Files(*.jgp; *.png; *.jpeg; *.bmp)";
            if ((openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)&&(openFileDialog.FileName != null))
            {
                this.OriginalPicturePath = openFileDialog.FileName;
                this.OriginalPictureData = new ImageIO(this.OriginalPicturePath);
                this.OriginalPictureData.LockBits();
                this.OriginalPictureData.UnlockBits();
                this.OriginalPicture.Source = new BitmapImage(new Uri(this.OriginalPicturePath));
                this.AddInfo("open original picture " + this.OriginalPicturePath);
                this.AddInfo("Width: " + this.OriginalPictureData.Width.ToString());
                this.AddInfo("Height: " + this.OriginalPictureData.Height.ToString());
            }
        }

        // Bitmap转BitmapImage
        private BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        // UI 转换逻辑
        private void SavePicture_Click(object sender, RoutedEventArgs e)
        {
            if(this.ProcessedPictureData==null)
            {
                Error error = new Error();
                error.ShowDialog();
                this.AddError("No processed picture!");
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save processed picture...";
                saveFileDialog.Filter = "jpg文件(*.jpg)|*.jpg|png文件(*.png)|*.png|bmp文件(*.bmp)|*.bmp";
                if ((saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) && (saveFileDialog.FileName != null))
                {
                    this.ProcessedPicturePath = saveFileDialog.FileName;
                    this.ProcessedPictureData.Path = this.ProcessedPicturePath;
                    this.ProcessedPictureData.SaveImage(this.ProcessedPicturePath.Substring(this.ProcessedPicturePath.Length - 3));
                    this.AddInfo("save processed picture " + this.ProcessedPicturePath);
                    this.AddInfo("Width: " + this.ProcessedPictureData.Width.ToString());
                    this.AddInfo("Height: " + this.ProcessedPictureData.Height.ToString());
                }
            }
        }

        private void TransformType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.HideDistortionOption();
            this.HideWarpOption();
            this.HideTPSOption();
            this.HideInterpolationOption();
            if (this.TransformType.SelectedItem == this.TransformDistortion)
            {
                this.ShowDistortionOption();
            }

            if (this.TransformType.SelectedItem == this.TransformWarp)
            {
                this.ShowWarpOption();
            }

            if (this.TransformType.SelectedItem == this.TransformTPS)
            {
                this.ShowTPSOption();
            }

            if (this.TransformType.SelectedItem == this.TransformNone)
            {
                this.ShowInterpolationOption();
            }
        }

        private void InterpolationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.HideNearestNeighborOption();
            this.HideBiLinearOption();
            this.HideBiCubicOption();
            if (this.InterpolationType.SelectedItem == this.InterpolationNearestNeighbor)
            {
                this.ShowNearestNeighborOption();
            }

            if (this.InterpolationType.SelectedItem == this.InterpolationBiLinear)
            {
                this.ShowBiLinearOption();
            }

            if (this.InterpolationType.SelectedItem == this.InterpolationBiCubic)
            {
                this.ShowBiCubicOption();
            }
        }

        private void SourceImageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.SourceImageSelection.SelectedIndex!=-1)
            {
                this.OriginalPictureData = this.TPSSelectionImage((this.SourceImageSelection.SelectedIndex+1).ToString());
                this.OriginalPictureData.LockBits();
                this.OriginalPictureData.UnlockBits();
                this.OriginalPicturePath = this.OriginalPictureData.Path;
                this.OriginalPicture.Source = new BitmapImage(new Uri(this.OriginalPicturePath));
                this.TargetHeightValue.Text = this.OriginalPictureData.Height.ToString();
                this.TargetWidthValue.Text = this.OriginalPictureData.Width.ToString();
                this.AddInfo("open source picture " + this.OriginalPicturePath);
                this.AddInfo("Width: " + this.OriginalPictureData.Width.ToString());
                this.AddInfo("Height: " + this.OriginalPictureData.Height.ToString());
            }
        }

        private void FaceImageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FaceImageSelection.SelectedIndex != -1)
            {
                this.FacePictureData = this.TPSSelectionImage((this.FaceImageSelection.SelectedIndex + 1).ToString());
                this.FacePictureData.LockBits();
                this.FacePictureData.UnlockBits();
                this.FacePicturePath = this.FacePictureData.Path;
                this.ProcessedPicture.Source = new BitmapImage(new Uri(this.FacePicturePath));
                this.AddInfo("open face picture " + this.FacePicturePath);
                this.AddInfo("Width: " + this.FacePictureData.Width.ToString());
                this.AddInfo("Height: " + this.FacePictureData.Height.ToString());
            }
        }

        private void ShowSourceMarked_Click(object sender, RoutedEventArgs e)
        {
            this.OriginalPicture.Source = this.BitmapToBitmapImage(this.OriginalMarkedPicture.Bitmap);
        }

        private void ShowFaceMarked_Click(object sender, RoutedEventArgs e)
        {
            this.ProcessedPicture.Source = this.BitmapToBitmapImage(this.FaceMarkedPicture.Bitmap);
        }

        private void ShowTPS_Click(object sender, RoutedEventArgs e)
        {
            this.OriginalPicture.Source = this.BitmapToBitmapImage(this.ProcessedPictureData.Bitmap);
        }

        private void SaveTPS_Click(object sender, RoutedEventArgs e)
        {
            if (this.ProcessedPictureData == null)
            {
                Error error = new Error();
                error.ShowDialog();
                this.AddError("No processed picture!");
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save processed picture...";
                saveFileDialog.Filter = "jpg文件(*.jpg)|*.jpg|png文件(*.png)|*.png|bmp文件(*.bmp)|*.bmp";
                if ((saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) && (saveFileDialog.FileName != null))
                {
                    this.ProcessedPicturePath = saveFileDialog.FileName;
                    this.ProcessedPictureData.Path = this.ProcessedPicturePath;
                    this.ProcessedPictureData.SaveImage(this.ProcessedPicturePath.Substring(this.ProcessedPicturePath.Length - 3));
                    this.AddInfo("save processed picture " + this.ProcessedPicturePath);
                    this.AddInfo("Width: " + this.ProcessedPictureData.Width.ToString());
                    this.AddInfo("Height: " + this.ProcessedPictureData.Height.ToString());
                }
            }
        }
    }
}
