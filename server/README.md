# Ubuntu crew workstation

The static studio stays on GitHub Pages. `desktop/remote.json` selects the TLS WebSocket endpoint. The desktop is real Ubuntu 24.04 LTS with Xfce and Yaru, rather than the heavier default GNOME session. It is a single shared, password-protected crew session. The public local Python workspace remains independent.

## Existing deployment

- SSH alias: `awam-prod` (existing user-managed key).
- LXD instance: `backbenchers-desktop`, unprivileged Ubuntu 24.04 image. Nested namespaces are enabled only on this guest so WebKit can run its own sandbox; the browser sandbox is not disabled. This broadens the guest feature surface, as described in [Canonical’s LXD hardening guidance](https://canonical.com/lxd/docs/default/howto/security_harden/). The container is not privileged.
- Profile: `backbenchers`; 1 CPU, 1400 MiB RAM, 512 processes; dir storage pool `backbenchers`.
- Private network: `bbstudio0`, `10.197.44.1/24`; reserved guest address `10.197.44.177`.
- Guest user: `studio`. Its home directory persists. No host home, SSH keys, or production app directories are mounted.
- Guest services: `backbenchers-vnc` (localhost 5901) and `backbenchers-websocket` (private network 6080).
- Public endpoint: `wss://backbenchers.3-0-126-159.nip.io/websockify` via Nginx on 443. A valid Let's Encrypt certificate is managed by the host's existing Certbot renewal.
- Nginx configuration: `/etc/nginx/sites-available/backbenchers-desktop`. Other existing sites remain separate.
- UFW additions are scoped to DHCP/DNS on `bbstudio0` and forwarding from that bridge through `ens5`.

Installed guest packages include `xfce4 xfce4-terminal mousepad tigervnc-standalone-server tigervnc-tools websockify dbus-x11 xfonts-base yaru-theme-gtk yaru-theme-icon fonts-ubuntu sudo git python3 python3-pip nodejs npm curl ca-certificates epiphany-browser xdg-utils xdg-desktop-portal-gtk dbus-user-session at-spi2-core`.

`ubuntu-desktop-setup.sh` configures services after package installation. PAM creates the proper desktop runtime directory; portal and D-Bus packages support sandboxed GUI applications. `studio-browser` selects software rendering for this server without a GPU. It does not contain or create credentials. Provision the VNC password using `tigervncpasswd -f` through private stdin, write `/home/studio/.vnc/passwd` owned by studio with mode 600, and set the guest Linux password separately. VNC authentication uses its protocol's eight-character password limit; TLS protects transport. Nginx limits connection attempts and restricts WebSocket browser origins. Do not commit passwords or expose 5901/6080 publicly. The guest sudo permission applies only within the isolated container.

The existing password is kept in the owner's private local inspection folder, outside this repository. Change it through private administration, not a public issue or commit. This shared desktop is intended for the crew, not anonymous visitors or private individual accounts.

## Operations

```sh
ssh awam-prod 'sudo lxc list backbenchers-desktop'
ssh awam-prod 'sudo lxc exec backbenchers-desktop -- systemctl status backbenchers-vnc backbenchers-websocket --no-pager'
ssh awam-prod 'sudo lxc exec backbenchers-desktop -- journalctl -u backbenchers-vnc -u backbenchers-websocket -n 40 --no-pager'
```

Disconnecting a browser leaves applications running. Screen power only changes the studio display. Restart guest services if the session is stuck; restarting VNC closes running GUI applications. Back up the container or `/home/studio` before upgrades. The initial deployment does not add a scheduled backup. Apply Ubuntu security updates as part of normal server maintenance. The nip.io hostname depends on the current public IP; migrate it to a studio-owned domain when one is assigned.

To disable the integration, set `enabled` to `false` in `desktop/remote.json`, publish the site, and stop the two guest services or the dedicated container. This leaves other host applications untouched.
