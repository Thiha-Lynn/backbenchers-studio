StPageFlip 2.0.7 by Oleg Litovski (MIT), from the official page-flip npm package.
Source: https://github.com/Nodlik/StPageFlip
Local changes: cancel its animation frame and detach the window resize handler on destroy; copy PDF canvas pixels when creating portrait-mode temporary pages. The reader keeps only nearby pages mounted.
