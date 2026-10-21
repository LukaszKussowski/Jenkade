# Jenkade

Jenkade (**Jen**kins fa**cade**) is a local desktop application for working with Jenkins servers: browsing Jobs, starting and following Builds.

Each term lists its Polish UI wording, which `Strings.pl.resx` must follow.

## Language

**Connection**:
A user-configured access to one Jenkins server: its address and the identity Jenkade acts as.
_Avoid_: server, instance, profile, account
_Polish UI_: Połączenie (not „serwer”, „profil”, „konto”)

**Job**:
A runnable task defined in Jenkins; may live inside a Folder.
_Avoid_: task, project, pipeline (as a general name)
_Polish UI_: Job (not „zadanie”, „projekt”)

**Folder**:
A container grouping Jobs and other Folders in a Jenkins server's tree.
_Avoid_: directory, group, view
_Polish UI_: Folder (not „katalog”, „grupa”)

**Queued build**:
A request to run a Job that Jenkins has accepted but not yet started; it has no number and becomes a Build when it starts.
_Avoid_: pending build, queue item, scheduled build
_Polish UI_: Oczekujący build

**Build**:
A single execution of a Job, identified by a number, with a Result and a Console log.
_Avoid_: run, execution, compilation
_Polish UI_: Build, plural buildy (not „uruchomienie”, „kompilacja”, „wykonanie”)

**Result**:
The final outcome of a finished Build: Success, Failure, Unstable (the Build completed but e.g. tests failed), Aborted or Not built (the Build was not executed). A Build in progress has no Result yet.
_Avoid_: status, state (for a finished Build)
_Polish UI_: Wynik: Sukces, Błąd, Niestabilny, Przerwany, Niezbudowany (not „status”, „stan”)

**Build parameter**:
A value supplied when starting a Build, as defined by the Job.
_Avoid_: argument, variable
_Polish UI_: Parametr builda (not „argument”, „zmienna”)

**Console log**:
The text output of a Build, growing while the Build is in progress.
_Avoid_: output, build log
_Polish UI_: Log konsoli (not „output”, „wyjście”)

**Abort**:
Stopping a Build in progress or a Queued build at the user's request.
_Avoid_: cancel, stop, kill
_Polish UI_: Przerwij / Przerwanie (not „anuluj”, „zatrzymaj”)
