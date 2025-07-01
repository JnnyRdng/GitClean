package utils

import (
	"os/exec"
	"strings"
)

func runGitCommand(workingDir string, args ...string) (string, error) {
	cmd := exec.Command("git", args...)
	cmd.Dir = workingDir
	out, err := cmd.CombinedOutput()
	var stringed = string(out)
	if err != nil {
		return stringed, err
	}
	return stringed, nil
}

func gitFetch(directory string) (string, error) {
	return runGitCommand(directory, "fetch", "--prune")
}

func GitTopLevel(directory string) (string, error) {
	return runGitCommand(directory, "rev-parse", "--show-toplevel")
}

func GitCurrentBranchName(directory string) (string, error) {
	return runGitCommand(directory, "rev-parse", "--abbrev-ref", "HEAD")
}

func GitGetBranches(directory string, allBranches bool) ([]string, error) {
	fetch, fetchErr := gitFetch(directory)
	if fetchErr != nil {
		return []string{fetch}, fetchErr
	}
	out, err := runGitCommand(directory, "branch", "-vv")
	if err != nil {
		return []string{out}, err
	}
	var lines = strings.Split(out, "\n")
	for i, line := range lines {
		li := &lines[i]
		*li = strings.TrimSpace(line)
		if strings.HasPrefix(line, "*") {
			*li = ""
		}
		if !allBranches {
			if !strings.Contains(line, ": gone]") {
				*li = ""
			}
		}
	}
	lines = Filter(lines, func(line string) bool {
		return line != ""
	})
	for i, line := range lines {
		split := strings.Split(line, " ")
		lines[i] = strings.TrimSpace(split[0])
	}
	return lines, nil
}

func TryDeleteBranch(directory string, branch string, isDryRun bool) (string, error) {
	if isDryRun {
		return "Didn't actually delete it", nil
	}
	return runGitCommand(directory, "-c", "advice.forceDeleteBranch=false", "branch", "-d", branch)
}

func ForceDeleteBranches(directory string, branches []string) (string, error) {
	return runGitCommand(directory, "branch", "-D", strings.Join(branches, " "))
}
