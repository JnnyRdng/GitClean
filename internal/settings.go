package internal

import (
	"log"
	"os"
	"path/filepath"
	"strings"

	"github.com/spf13/pflag"
)

func ParseSettings() *Settings {
	dryRun := pflag.BoolP("dry-run", "d", false, "Dry run, don't delete anything")
	allBranches := pflag.BoolP("all", "a", false, "Choose from any branch, not just those deleted on the remote")
	pflag.Parse()
	var dir = ""
	if len(os.Args) > 1 {
		dir = os.Args[1]
	}
	if strings.HasPrefix(dir, "-") {
		dir = ""
	}
	settings := &Settings{
		DryRun:           *dryRun,
		AllBranches:      *allBranches,
		WorkingDirectory: dir,
	}
	if ok, err := settings.Validate(); !ok {
		log.Fatal(err)
	}
	return settings
}

type Settings struct {
	WorkingDirectory string
	AllBranches      bool
	DryRun           bool
}

func (s *Settings) Validate() (bool, string) {
	var path = s.WorkingDirectory
	if path == "" {
		if p, err := os.Getwd(); err != nil {
			return false, "Could not get current working directory"
		} else {
			path = p
		}
	}
	if strings.HasPrefix(path, "~") {
		if home, err := os.UserHomeDir(); err != nil {
			return false, "Could not get home directory"
		} else {
			if len(path) > 2 {
				path = home + path[1:]
			} else {
				path = home
			}
		}
	}
	abs, absErr := filepath.Abs(path)
	if absErr != nil {
		return false, "Bad filepath"
	}
	_, err := os.Stat(abs)
	if err != nil {
		return false, err.Error() + " " + abs
	}
	top, err := GitTopLevel(abs)
	if err != nil {
		return false, err.Error() + " " + top
	}
	s.WorkingDirectory = strings.TrimSpace(top)
	return true, ""
}
