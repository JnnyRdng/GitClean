# GitClean

Removes local branches in a git repository that no longer exist on the remote.
Can optionally be used to interactively delete any git branch other than the current.

## Help

```bash
> gitclean -h
DESCRIPTION:
Removes local branches in a git repository that no longer exist on the remote

USAGE:
    gitclean [DIRECTORY] [OPTIONS]

ARGUMENTS:
    [DIRECTORY]    Optional target directory. If omitted, the current working directory is used. Must be within a git repository

OPTIONS:
    -h, --help       Prints help information
    -d, --dry-run    Dry run. No branches will be deleted
    -a, --all        Dangerous. Select from all branches
```

## Usage

```bash
> gitclean [DIRECTORY] [OPTIONS]
```
