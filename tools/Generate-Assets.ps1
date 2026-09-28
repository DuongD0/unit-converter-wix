<#
.SYNOPSIS
    Generates the application icon and the Microsoft Store logo images from one drawing routine,
    so every size stays consistent. Re-run after changing the design.
.NOTES
    Requires Windows PowerShell 5.1 (System.Drawing).
#>
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path $PSScriptRoot -Parent
$iconPath = Join-Path $root 'src\UnitConverter.App\app.ico'
$imagesDir = Join-Path $root 'packaging\UnitConverter.Package\Images'
New-Item -ItemType Directory -Force $imagesDir | Out-Null

$accent = [System.Drawing.Color]::FromArgb(0, 99, 177)

# Draws the logo: a rounded blue tile with two opposing arrows, centred in a width x height canvas.
function New-Logo([int] $Width, [int] $Height, [double] $Scale = 1.0) {
    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = 'AntiAlias'
    $g.Clear([System.Drawing.Color]::Transparent)

    $size = [Math]::Min($Width, $Height) * $Scale
    $x = ($Width - $size) / 2
    $y = ($Height - $size) / 2
    $radius = $size * 0.22

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($x, $y, $radius * 2, $radius * 2, 180, 90)
    $path.AddArc($x + $size - $radius * 2, $y, $radius * 2, $radius * 2, 270, 90)
    $path.AddArc($x + $size - $radius * 2, $y + $size - $radius * 2, $radius * 2, $radius * 2, 0, 90)
    $path.AddArc($x, $y + $size - $radius * 2, $radius * 2, $radius * 2, 90, 90)
    $path.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush $accent), $path)

    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), ([Math]::Max(1.5, $size * 0.085))
    $pen.StartCap = 'Round'
    $pen.EndCap = 'Round'
    $left = $x + $size * 0.24
    $right = $x + $size * 0.76
    $head = $size * 0.13
    foreach ($row in @(@(0.38, 1), @(0.62, -1))) {
        $ry = $y + $size * $row[0]
        $tip = if ($row[1] -eq 1) { $right } else { $left }
        $g.DrawLine($pen, $left, $ry, $right, $ry)
        $g.DrawLine($pen, $tip, $ry, $tip - $row[1] * $head, $ry - $head)
        $g.DrawLine($pen, $tip, $ry, $tip - $row[1] * $head, $ry + $head)
    }

    $g.Dispose()
    return $bitmap
}

function Save-Png([System.Drawing.Bitmap] $Bitmap, [string] $Path) {
    $Bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $Bitmap.Dispose()
}

# Store / MSIX visual assets (names match Package.appxmanifest).
Save-Png (New-Logo 50 50)            (Join-Path $imagesDir 'StoreLogo.png')
Save-Png (New-Logo 44 44)            (Join-Path $imagesDir 'Square44x44Logo.png')
Save-Png (New-Logo 150 150 0.66)     (Join-Path $imagesDir 'Square150x150Logo.png')
Save-Png (New-Logo 310 150 0.66)     (Join-Path $imagesDir 'Wide310x150Logo.png')
Save-Png (New-Logo 620 300 0.66)     (Join-Path $imagesDir 'SplashScreen.png')

# Multi-resolution .ico: an icon directory followed by PNG-encoded images (supported since Windows Vista).
$sizes = 16, 24, 32, 48, 64, 128, 256
$images = foreach ($s in $sizes) {
    $stream = New-Object System.IO.MemoryStream
    $bitmap = New-Logo $s $s
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    , $stream.ToArray()
}

$file = [System.IO.File]::Create($iconPath)
$writer = New-Object System.IO.BinaryWriter $file
$writer.Write([UInt16]0); $writer.Write([UInt16]1); $writer.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $dim = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }   # 0 means 256 in the ICO format
    $writer.Write([byte]$dim); $writer.Write([byte]$dim)
    $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([UInt16]1); $writer.Write([UInt16]32)
    $writer.Write([UInt32]$images[$i].Length); $writer.Write([UInt32]$offset)
    $offset += $images[$i].Length
}
foreach ($bytes in $images) { $writer.Write($bytes) }
$writer.Dispose()

Write-Host "Icon:   $iconPath"
Write-Host "Images: $imagesDir"
