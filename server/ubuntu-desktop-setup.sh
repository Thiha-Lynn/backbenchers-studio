#!/bin/bash
set -euo pipefail
id studio >/dev/null 2>&1 || useradd -m -s /bin/bash studio
usermod -aG sudo studio
install -d -m 700 -o studio -g studio /home/studio/.vnc
install -d -o studio -g studio /home/studio/Desktop /home/studio/Projects /home/studio/.config/xfce4/xfconf/xfce-perchannel-xml
cat > /home/studio/.vnc/xstartup <<'EOF'
#!/bin/sh
unset SESSION_MANAGER
unset DBUS_SESSION_BUS_ADDRESS
export XDG_SESSION_TYPE=x11
export XDG_CURRENT_DESKTOP=XFCE
exec dbus-run-session -- startxfce4
EOF
chmod 700 /home/studio/.vnc/xstartup
cat > /home/studio/Desktop/Welcome.txt <<'EOF'
BACKBENCHERS STUDIO
Ubuntu 24.04 LTS · Crew workstation

This is a real Ubuntu desktop running in its own unprivileged container.
Open Terminal with Ctrl+Alt+T. Python, Node.js, Git and ordinary Linux commands work here.
Your files live in /home/studio and persist on the studio server.
The production host's files are not mounted into this desktop.

Code. Play. Flight.
Thomas · Hlaing · Merlin
EOF
cat > /home/studio/Projects/hello.py <<'EOF'
import platform
print("Welcome aboard, Backbenchers!")
print("Real Python:", platform.python_version())
print("System:", platform.system())
print("Crew:", ", ".join(["Thomas", "Hlaing", "Merlin"]))
EOF
cat > /home/studio/.config/xfce4/xfconf/xfce-perchannel-xml/xsettings.xml <<'EOF'
<?xml version="1.0" encoding="UTF-8"?><channel name="xsettings" version="1.0"><property name="Net" type="empty"><property name="ThemeName" type="string" value="Yaru-dark"/><property name="IconThemeName" type="string" value="Yaru"/></property><property name="Gtk" type="empty"><property name="FontName" type="string" value="Ubuntu 10"/></property></channel>
EOF
cat > /home/studio/.config/xfce4/xfconf/xfce-perchannel-xml/xfce4-keyboard-shortcuts.xml <<'EOF'
<?xml version="1.0" encoding="UTF-8"?><channel name="xfce4-keyboard-shortcuts" version="1.0"><property name="commands" type="empty"><property name="custom" type="empty"><property name="&lt;Primary&gt;&lt;Alt&gt;t" type="string" value="xfce4-terminal"/></property></property></channel>
EOF
chown -R studio:studio /home/studio
hostnamectl set-hostname backbenchers-studio
timedatectl set-timezone Asia/Bangkok
cat > /etc/systemd/system/backbenchers-vnc.service <<'EOF'
[Unit]
Description=Backbenchers Ubuntu crew desktop
After=network.target
[Service]
Type=simple
User=studio
Group=studio
Environment=HOME=/home/studio
WorkingDirectory=/home/studio
ExecStart=/usr/bin/tigervncserver :1 -fg -localhost yes -geometry 1280x720 -depth 24 -SecurityTypes VncAuth -PasswordFile /home/studio/.vnc/passwd -xstartup /home/studio/.vnc/xstartup
Restart=on-failure
RestartSec=5
TasksMax=384
[Install]
WantedBy=multi-user.target
EOF
cat > /etc/systemd/system/backbenchers-websocket.service <<'EOF'
[Unit]
Description=Backbenchers authenticated desktop WebSocket transport
After=backbenchers-vnc.service
Requires=backbenchers-vnc.service
[Service]
User=studio
ExecStart=/usr/bin/websockify 0.0.0.0:6080 127.0.0.1:5901
Restart=always
RestartSec=5
[Install]
WantedBy=multi-user.target
EOF
systemctl daemon-reload
