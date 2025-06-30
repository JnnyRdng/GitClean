package main

import (
	"fmt"
	"gitclean/internal"
	"log"

	"github.com/charmbracelet/bubbles/spinner"
	tea "github.com/charmbracelet/bubbletea"
	"github.com/charmbracelet/lipgloss"
)

func initApp() model {
	settings := internal.ParseSettings()
	spin := spinner.New()
	spin.Spinner = spinner.Jump
	spin.Style = lipgloss.NewStyle().Foreground(lipgloss.Color("#FF69B4"))

	return model{
		appState:    0,
		settings:    settings,
		spinner:     spin,
		selectedMap: make(map[int]struct{}),
	}
}

func main() {
	p := tea.NewProgram(initApp(), tea.WithAltScreen())
	_, err := p.Run()
	if err != nil {
		defer fmt.Printf("%v\n", err)
	}

}

var cerulean = lipgloss.Color("#8D989B") // blueish
var mantis = lipgloss.Color("#7DC95E")   // green
var red = lipgloss.Color("#DB5461")      // red
var purple = lipgloss.Color("#381D2A")   //blue
var white = lipgloss.Color("#F5F3F5")    //white

func GetContainer(width int) lipgloss.Style {
	return lipgloss.NewStyle().
		Padding(2, 4).
		Width(width)
}

type failedBranch struct {
	name   string
	reason string
}

type model struct {
	settings *internal.Settings

	width  int
	height int

	spinner spinner.Model

	appState         int
	branches         []string
	branchCursor     int
	selectedMap      map[int]struct{}
	selectedBranches []string
	failed           []failedBranch

	err error
}

func (m model) Init() tea.Cmd {
	return m.spinner.Tick
}

var style = lipgloss.NewStyle().
	Bold(true).
	Foreground(lipgloss.Color("#FAFAFA")).
	Background(lipgloss.Color("#7D56F4")).
	Padding(2, 4).
	Width(22)

func (m model) Update(msg tea.Msg) (tea.Model, tea.Cmd) {
	switch msgT := msg.(type) {
	case tea.KeyMsg:
		str := msgT.String()
		return m.HandleKeyboardInput(str)
	case tea.WindowSizeMsg:
		m.width = msgT.Width
		m.height = msgT.Height
	case appState:
		m.appState = msgT.next
		if m.appState == 1 {
			return m, m.startFetch()
		}
	case branchListMsg:
		m.branches = msgT.data
		m.appState = msgT.state.next
	default:
		var cmd tea.Cmd
		m.spinner, cmd = m.spinner.Update(msg)
		return m, cmd
	}
	return m, nil
}

func (m model) HandleKeyboardInput(input string) (tea.Model, tea.Cmd) {
	if input == "ctrl+c" || input == "q" || input == "esc" {
		log.Println("Exiting...")
		return m, tea.Quit
	}
	if m.appState == 0 && input == "enter" {
		return m, func() tea.Msg {
			return appState{m.appState + 1}
		}
	}
	if m.appState == 2 {
		switch input {
		case "up", "j":
			m.branchCursor = (m.branchCursor - 1 + len(m.branches)) % len(m.branches)
			return m, nil
		case "down", "k":
			m.branchCursor = (m.branchCursor + 1 + len(m.branches)) % len(m.branches)
			return m, nil
		case " ":
			if _, ok := m.selectedMap[m.branchCursor]; ok {
				delete(m.selectedMap, m.branchCursor)
			} else {
				m.selectedMap[m.branchCursor] = struct{}{}
			}
		case "enter":
			m.selectedBranches = m.GetSelected()
			m.appState++
			return m, nil
		}
	}

	if input == "enter" {
		return m, tea.Quit
	}

	return m, nil
}

type branchListMsg struct {
	data  []string
	state appState
}

type selectedBranchesMsg struct {
	data  []string
	state appState
}

type appState struct {
	next int
}

func (m model) startFetch() tea.Cmd {
	return func() tea.Msg {
		// branches, err := internal.GitGetBranches(m.settings.WorkingDirectory, m.settings.AllBranches)
		// if err != nil {
		// 	m.err = err
		// 	return m
		// }
		// return branchListMsg{branches, appState{2}}
		var words = []string{
			"fix-auth-bug",
			"feat-ui-tweak",
			"ref-log-clean",
			"opt-cache-hit",
			"test-api-post",
			"chore-deps-up",
			"hotfix-404",
			"rm-old-routes",
			"fix-typos",
			"feat-fake-branches",
			"test-the-whole-thing",
		}
		return branchListMsg{words, appState{2}}
	}
}

func (m model) GetSelected() []string {
	selectedBranches := make([]string, 0, len(m.selectedMap))
	for i := range m.selectedMap {
		if i >= 0 && i < len(m.branches) {
			selectedBranches = append(selectedBranches, m.branches[i])
		}
	}
	return selectedBranches
}

var codestyle = lipgloss.NewStyle().Foreground(red).Bold(true)

func (m model) View() string {

	if m.err != nil {
		log.Fatalln(m.err.Error())
	}
	container := GetContainer(m.width)
	style := lipgloss.NewStyle().Foreground(white)

	switch m.appState {
	case 0:
		{
			welcome := container.Background(purple).Align(lipgloss.Center).Render("gitclean")
			line := style.Render(fmt.Sprintf(
				"%s\n\nTo begin, press the %s key. This will perform a %s to determine which branches have been deleted.\n\nPress the %s key to exit.",
				welcome,
				codestyle.Render("<enter>"),
				codestyle.Render("git fetch --prune"),
				codestyle.Render("<esc>"),
			))
			result := container.Render(line)
			return result
		}
	case 1:
		{
			return container.Render(fmt.Sprintf("%s Running %s", m.spinner.View(), codestyle.Render("git fetch --prune")))
		}
	case 2:
		{
			return container.Render(m.RenderBranchList())
		}
	}

	var res string = fmt.Sprintf("There are %d branches to look at\n", len(m.selectedBranches))
	for _, b := range m.selectedBranches {
		res += fmt.Sprintf("  %s\n", b)
	}
	return container.Render(res)
}

var itemStyle = lipgloss.NewStyle().Foreground(cerulean).MarginRight(1)
var itemStyleSelected = itemStyle.Foreground(mantis).Bold(true).Underline(true)
var enumStyle = lipgloss.NewStyle().Foreground(cerulean)
var enumStyleSelected = enumStyle.Foreground(white)

const maxPageSize int = 10

func (m model) RenderBranchList() string {
	var s string = "Choose branches to remove:"
	clampedPageSize := min(len(m.branches), maxPageSize)
	page := internal.Paginate(m.branches, clampedPageSize)
	pageNum, currentPage := page.GetPageForIndex(m.branchCursor)
	if page.TotalPages > 1 {
		s += "\nPage"
		s += codestyle.Render(fmt.Sprintf(" %d", pageNum+1))
		s += " of"
		s += codestyle.Render(fmt.Sprintf(" %d", page.TotalPages))
	}
	s += "\n\n"
	for i, b := range currentPage {
		if b == "" {
			s += "\n"
			continue
		}
		ii := page.GetIndex(pageNum, i)
		if m.branchCursor == ii {
			s += "> "
		} else {
			s += "  "
		}
		_, ok := m.selectedMap[ii]
		if ok {
			s += enumStyleSelected.Render("[x]  ")
		} else {
			s += enumStyle.Render("[ ]  ")
		}
		if ok {
			s += itemStyleSelected.Render(b)
		} else {
			s += itemStyle.Render(b)
		}

		s += "\n"
	}

	s += "\nPress "
	s += codestyle.Render("[up]/[down]")
	s += " to scroll.\nPress "
	s += codestyle.Render("[space]")
	s += " to toggle.\n"
	s += "Press "
	s += codestyle.Render("[enter]")
	s += " to confirm."

	return s
}
