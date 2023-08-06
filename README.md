# AnimatedImage

A simple library to display animated GIF images and animated PNG images in WPF and AvaloniaUI, usable in XAML or in code.

## How to use (WPF)

It's very easy to use: in XAML, instead of setting the `Source` property, set the `AnimatedSource` attached property to the image you want:

```xml
<Window x:Class="WpfAnimatedGif.Demo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:anim="https://github.com/whistyun/AnimatedImage.Wpf"
        Title="MainWindow" Height="350" Width="525">
    <Grid>
        <Image anim:ImageBehavior.AnimatedSource="Images/animated.gif" />
```

You can also specify the repeat behavior (the default is `0x`, which means it will use the repeat count from the GIF metadata):

```xml
<Image anim:ImageBehavior.RepeatBehavior="3x"
       anim:ImageBehavior.AnimatedSource="Images/animated.gif" />
```

And of course you can also set the image in code:

```csharp
var image = new BitmapImage();
image.BeginInit();
image.UriSource = new Uri(fileName);
image.EndInit();
ImageBehavior.SetAnimatedSource(img, image);
```

## How to use (AvaloniaUI)

It has some compatibility with WPF: set the `AnimatedSource` attached property to the image you want:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:anim="https://github.com/whistyun/AnimatedImage.Avalonia"
        Title="MainWindow" Height="350" Width="525">
    <Grid>
        <Image anim:ImageBehavior.AnimatedSource="Images/animated.gif" />
```

You can also specify the repeat behavior (the default is `0x`, which means it will use the repeat count from the GIF metadata):

```xml
<Image anim:ImageBehavior.RepeatBehavior="3x"
       anim:ImageBehavior.AnimatedSource="Images/animated.gif" />
```
