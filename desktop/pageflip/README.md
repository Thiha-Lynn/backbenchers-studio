StPageFlip 2.0.7 by Oleg Litovski (MIT), from the official page-flip npm package.
Source: https://github.com/Nodlik/StPageFlip
Local changes: cancel its animation frame and detach the window resize handler on destroy; copy PDF canvas pixels when creating portrait-mode temporary pages. The reader keeps only nearby pages mounted.

The room reader captures pointer gestures directly for immediate mouse/touch folds. `stopMove(commit = null)` accepts an optional explicit completion decision, so a one-page mobile swipe can complete before the finger crosses the offscreen spine. An omitted argument preserves the original geometric behavior.
